// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.Extensions.Logging;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordRoleMenuService : IDiscordRoleMenuService
    {
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly IRoleMenuService _roleMenuService;
        private readonly ILogger<DiscordRoleMenuService> _logger;
        private readonly IDiscordEmbedProvider _embedProvider;

        public DiscordRoleMenuService(IDiscordService discord, IGuildService guildService,
            IRoleMenuService roleMenuService, ILogger<DiscordRoleMenuService> logger,
            IDiscordEmbedProvider embedProvider)
        {
            _discord = discord;
            _guildService = guildService;
            _roleMenuService = roleMenuService;
            _logger = logger;
            _embedProvider = embedProvider;
        }

        public async Task<(DiscordEmbed Embed, bool IsSuccess)> CreateRoleMenuAsync([NotNull] InteractionContext ctx, RoleMenuAddReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!ctx.Member.IsAdmin()) throw new DiscordNotAuthorizedException();

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(ctx.Guild.Id));
            if (guild is null) throw new NotFoundException();

            var intr = _discord.Client.GetInteractivity();
            int loopCount = 0;

            var mainMenu = new DiscordEmbedBuilder();
            mainMenu.WithAuthor($"Role menu configurator menu");
            mainMenu.WithDescription("Please select an option below to create your role menu!");
            mainMenu.WithColor(new DiscordColor(guild.EmbedHexColor));

            var resultEmbed = new DiscordEmbedBuilder();
            var wbhk = new DiscordWebhookBuilder();

            var mainButton = new DiscordButtonComponent(ButtonStyle.Primary, nameof(RoleMenuButton.RoleMenuAddOptionButton),
                "Add an option", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, "heavy_plus_sign")));
            var mainMsg = await ctx.EditResponseAsync(wbhk.AddEmbed(mainMenu.Build()).AddComponents(mainButton));

            var waitResult = await intr.WaitForButtonAsync(mainMsg,
                x => x.User.Id == mainMsg.Author.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOptionButton), TimeSpan.FromMinutes(1));

            if (waitResult.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed(req.Name, true)));
                return (null, false);
            }

            var roleMenu = new RoleMenu
            {
                CreatorId = ctx.Member.Id,
                GuildId = ctx.Guild.Id,
                LastEditById = ctx.Member.Id,
                Name = req.Name,
                Text = req.Text
            };
            roleMenu.CustomSelectComponentId = $"role_menu_{ctx.Guild.Id}_{roleMenu.Id}";

            while (true)
            {
                if (loopCount > 25) return (resultEmbed, true);

                (DiscordEmbedBuilder Embed, RoleMenu CurrentMenu) result;

                result = await AddNewOptionAsync(ctx, intr, resultEmbed, roleMenu);

                if (result.CurrentMenu is null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed(req.Name, true)));
                    return (null, false);

                }
                
                var finalizeMsg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed.Build())
                    .AddEmbed(mainMenu.Build())
                    .AddComponents(mainButton)
                    .AddComponents(GetMainFinalizeButton(ctx.Client)));

                var waitForFinalButtonTask = intr.WaitForButtonAsync(finalizeMsg,
                    x => x.User.Id == mainMsg.Author.Id && x.Id == nameof(RoleMenuButton.RoleMenuFinalizeButton), TimeSpan.FromMinutes(1));

                var waitForNextOptionTask = intr.WaitForButtonAsync(mainMsg,
                    x => x.User.Id == mainMsg.Author.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOptionButton), TimeSpan.FromMinutes(1));

                var taskAggregate = await Task.WhenAny(new[] {waitForFinalButtonTask, waitForNextOptionTask});

                if (taskAggregate.Result.TimedOut)
                    return (resultEmbed, false);
                if (taskAggregate.Result.Result.Id == nameof(EmbedConfigButton.EmbedConfigFinalButton))
                    break;

                loopCount++;
            }

            foreach (var option in roleMenu.RoleMenuOptions)
            {
                DiscordRole role = await ctx.Guild.CreateRoleAsync(option.Name);
                option.RoleId = role.Id;
                option.CustomSelectOptionValueId = $"role_menu_option_{role.Id}";
                await Task.Delay(300);
            }

            await _roleMenuService.AddAsync(roleMenu, true);

            var embed = new DiscordEmbedBuilder();
            return (embed, true);
        }

        public async Task<(DiscordWebhookBuilder Builder, string Text)> GetAsync(RoleMenuGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = null;
            if (req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            else if (req.Id.HasValue)
            {
                var tag = await _roleMenuService.GetAsync<RoleMenu>(req.Id.Value);
                if (tag is null) throw new NotFoundException("Role menu with given Id was not found");
                guild = await _discord.Client.GetGuildAsync(tag.GuildId);
            }

            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await GetAsync(guild, requestingUser, req);
        }

        public async Task<(DiscordWebhookBuilder Builder, string Text)> GetAsync(InteractionContext ctx, RoleMenuGetReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await GetAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<(DiscordWebhookBuilder Builder, string Text)> GetAsync(DiscordGuild guild, DiscordMember requestingUser,
            RoleMenuGetReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            RoleMenu roleMenu;
            if (requestingUser.IsBotOwner(_discord.Client))
            {
                roleMenu = req.Id.HasValue
                    ? await _roleMenuService.GetAsync<RoleMenu>(req.Id.Value)
                    : await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(new Specification<RoleMenu>(x => x.Name == req.Name));
            }
            else
            {
                var guildCfg =
                    await _guildService.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByIdSpec(guild.Id));
                if (guildCfg is null)
                    throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

                if (requestingUser.Guild.Id != guild.Id) throw new DiscordNotAuthorizedException();

                roleMenu = req.Id.HasValue
                    ? await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                        new RoleMenuByIdWithOptionsSpec(req.Id.Value))
                    : await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                        new RoleMenuByNameAndGuildWithOptionsSpec(req.Name, req.GuildId.Value));
            }

            if (roleMenu is null) throw new NotFoundException("Role menu not found");
            if (roleMenu.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
                throw new DisabledEntityException("Found role menu is disabled");

            var builder = new DiscordWebhookBuilder();

            List<DiscordSelectComponentOption> options = roleMenu.RoleMenuOptions.Select(option =>
                    new DiscordSelectComponentOption(option.Name, option.CustomSelectOptionValueId,
                        string.IsNullOrWhiteSpace(option.Description) ? null : option.Description, false,
                        new DiscordComponentEmoji(string.IsNullOrWhiteSpace(option.Emoji)
                            ? null
                            : DiscordEmoji.FromName(_discord.Client, option.Emoji))))
                .ToList();

            var select = new DiscordSelectComponent(roleMenu.CustomSelectComponentId, "Choose a role!", options);

            builder.AddComponents(select);

            if (roleMenu.EmbedConfig is null)
            {
                return (null, roleMenu.Text);
            }

            var embed = _embedProvider.ConfigureEmbed(roleMenu.EmbedConfig).Build();
            return (builder.AddEmbed(embed), roleMenu.Text);
        }

        public async Task<(DiscordWebhookBuilder Builder, string Text)> SendAsync(RoleMenuSendReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = null;
            if (req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            else if (req.Id.HasValue)
            {
                var roleMenu = await _roleMenuService.GetAsync<RoleMenu>(req.Id.Value);
                if (roleMenu is null) throw new NotFoundException("Role menu with given Id was not found");
                guild = await _discord.Client.GetGuildAsync(roleMenu.GuildId);
            }

            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            DiscordChannel target = guild.GetChannel(req.ChannelId);

            return await SendAsync(guild, requestingUser, target, req);
        }

        public async Task<(DiscordWebhookBuilder Builder, string Text)> SendAsync(InteractionContext ctx, RoleMenuSendReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await SendAsync(ctx.Guild, ctx.Member, ctx.ResolvedChannelMentions[0], req);
        }

        private async Task<(DiscordWebhookBuilder Embed, string Text)> SendAsync(DiscordGuild guild, DiscordMember requestingUser,
            DiscordChannel target, RoleMenuSendReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (req is null) throw new ArgumentNullException(nameof(req));

            RoleMenu roleMenu;
            if (requestingUser.IsBotOwner(_discord.Client))
            {
                roleMenu = req.Id.HasValue
                    ? await _roleMenuService.GetAsync<RoleMenu>(req.Id.Value)
                    : await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(new Specification<RoleMenu>(x => x.Name == req.Name));
            }
            else
            {
                var guildCfg =
                    await _guildService.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByIdSpec(guild.Id));
                if (guildCfg is null)
                    throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

                if (requestingUser.Guild.Id != guild.Id) throw new DiscordNotAuthorizedException();

                roleMenu = req.Id.HasValue
                    ? await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                        new RoleMenuByIdWithOptionsSpec(req.Id.Value))
                    : await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                        new RoleMenuByNameAndGuildWithOptionsSpec(req.Name, req.GuildId.Value));
            }

            if (roleMenu is null) throw new NotFoundException("Role menu not found");
            if (roleMenu.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
                throw new DisabledEntityException("Role menu is disabled");

            var builder = new DiscordWebhookBuilder();

            List<DiscordSelectComponentOption> options = roleMenu.RoleMenuOptions.Select(option =>
                new DiscordSelectComponentOption(option.Name, option.CustomSelectOptionValueId,
                    string.IsNullOrWhiteSpace(option.Description) ? null : option.Description, false,
                    new DiscordComponentEmoji(string.IsNullOrWhiteSpace(option.Emoji)
                        ? null
                        : DiscordEmoji.FromName(_discord.Client, option.Emoji))))
                .ToList();

            var select = new DiscordSelectComponent(roleMenu.CustomSelectComponentId, "Choose a role!", options);

            builder.AddComponents(select);

            if (roleMenu.EmbedConfig is null)
            {
                await target.SendMessageAsync(roleMenu.Text);
                return (null, roleMenu.Text);
            }

            builder.AddEmbed(_embedProvider.ConfigureEmbed(roleMenu.EmbedConfig).Build());
            await target.SendMessageAsync(new DiscordMessageBuilder().WithContent(builder.Content)
                .AddComponents(builder.Components));
            return (builder, roleMenu.Text);
        }

        public async Task HandleOptionSelectionAsync(ComponentInteractionCreateEventArgs args)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (!long.TryParse(args.Id, out long parsed)) return;

            //var roleMenu = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(new RoleMenuByIdWithOptionsSpec(parsed));

            try
            {
                if (!ulong.TryParse(args.Values[0].Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim(),
                    out var parsedValue)) return;

                /*var option = roleMenu.RoleMenuOptions.FirstOrDefault(x => x.RoleId == parsedValue);

                if (option is null) return;*/

                var role = args.Guild.GetRole(parsedValue); //.GetRole(option.RoleId);

                var member = await args.Guild.GetMemberAsync(args.User.Id);

                if (member.Roles.Any(x => x.Id == role.Id))
                    await member.RevokeRoleAsync(role);
                else
                    await member.GrantRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to grant/revoke role: {ex.GetFullMessage()}");
            }
        }

        private async Task<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)> AddNewOptionAsync(InteractionContext ctx,
            InteractivityExtension intr, DiscordEmbedBuilder currentResult, RoleMenu roleMenu)
        {
            var embed = new DiscordEmbedBuilder().WithFooter("Lisbeth configuration");

            embed.WithAuthor("Role menu option configurator");
            embed.AddField("Instructions",
                "Please respond to this message using this template: @name@ roleName @endName@ @desc@ description @endDesc@ @emoji@ :emoji: @endEmoji@");
            embed.AddField("Example",
                "@name@ Special role @endName@ @desc@ Super description @endDesc@ @emoji@ :smiling_imp: @endEmoji@");
            embed.AddField("Requirements", "Role name is mandatory.");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));

            var waitResult = await intr.WaitForMessageAsync(
                x => x.ChannelId == ctx.Channel.Id && x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(1));

            if (waitResult.TimedOut)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed("")));
                return (currentResult, null);
            }

            if (string.IsNullOrWhiteSpace(waitResult.Result.Content.Trim()))
                throw new ArgumentException("Response can't be empty or null.");

            string name = waitResult.Result.Content.GetStringBetween("@name@", "@endName@").Trim();
            string desc = waitResult.Result.Content.GetStringBetween("@desc@", "@endDesc@").Trim();
            string emoji = waitResult.Result.Content.GetStringBetween("@emoji@", "@endEmoji@").Trim();

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            RoleMenuOption newOption = new RoleMenuOption {Name = name};
            if (!string.IsNullOrWhiteSpace(desc)) newOption.Description = desc;

            newOption.Emoji = string.IsNullOrWhiteSpace(emoji) switch
            {
                false when DiscordEmoji.TryFromName(ctx.Client, emoji, true, out _) => emoji,
                false when !DiscordEmoji.TryFromName(ctx.Client, emoji, true, out _) => throw new ArgumentException(
                    "Provided emoji is invalid"),
                _ => newOption.Emoji
            };

            roleMenu.AddRoleMenuOption(newOption);

            currentResult.AddField(name,
                $"Additional settings (if any): {(string.IsNullOrWhiteSpace(emoji) ? "" : $"Emoji - {emoji}")}{(string.IsNullOrWhiteSpace(desc) ? "" : $", Description - {desc}")}");

            return (currentResult, roleMenu);
        }

        private DiscordEmbed GetTimedOutEmbed(string idOrName, bool isFirst = false)
        {
            var timedOut = new DiscordEmbedBuilder();
            timedOut.WithAuthor($"Role menu configurator menu for Id: {idOrName}");
            timedOut.WithDescription(
                $"Your interaction timed out, please {(!isFirst ? "decide whether you want to save current version or abort and " : "")} try again!");
            timedOut.WithFooter("Lisbeth configuration");
            timedOut.WithColor(new DiscordColor("#26296e"));

            return timedOut.Build();
        }

        private static DiscordButtonComponent GetMainFinalizeButton(DiscordClient client)
        {
            return new(ButtonStyle.Primary, nameof(EmbedConfigButton.EmbedConfigFinalButton), "Finalize", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:")));
        }
    }
}