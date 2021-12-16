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
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Microsoft.Extensions.Logging;
using MikyM.Common.DataAccessLayer;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;
using System.Collections.Generic;
using Lisbeth.Bot.Application.Discord.Extensions;
using MikyM.Common.Utilities.Extensions;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordRoleMenuService : IDiscordRoleMenuService
{
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordRoleMenuService> _logger;
    private readonly IRoleMenuService _roleMenuService;

    public DiscordRoleMenuService(IDiscordService discord, IGuildDataService guildDataService,
        IRoleMenuService roleMenuService, ILogger<DiscordRoleMenuService> logger,
        IDiscordEmbedProvider embedProvider)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _roleMenuService = roleMenuService;
        _logger = logger;
        _embedProvider = embedProvider;
    }

    public async Task<Result<DiscordEmbed>> CreateRoleMenuAsync(InteractionContext ctx, RoleMenuAddReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!ctx.Member.IsAdmin()) throw new DiscordNotAuthorizedException();

        var guildResult = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(ctx.Guild.Id));
        if (!guildResult.IsDefined()) return Result<DiscordEmbed>.FromError(guildResult);

        var count = await _roleMenuService.LongCountAsync(
            new RoleMenuByNameAndGuildWithOptionsSpec(req.Name, req.GuildId));

        if (!count.IsDefined(out var countRes) || countRes >= 1)
            return new SameEntityNamePerGuildError("Role menu", req.Name ?? "null");

        var intr = _discord.Client.GetInteractivity();
        int loopCount = 0;

        var mainMenu = new DiscordEmbedBuilder();
        mainMenu.WithAuthor("Role menu configurator menu");
        mainMenu.WithDescription("Please select an option below to create your role menu!");
        mainMenu.WithColor(new DiscordColor(guildResult.Entity.EmbedHexColor));

        var resultEmbed = new DiscordEmbedBuilder();
        var wbhk = new DiscordWebhookBuilder();

        var mainButton = new DiscordButtonComponent(ButtonStyle.Primary,
            nameof(RoleMenuButton.RoleMenuAddOption),
            "Add an option", false,
            new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":heavy_plus_sign:")));
        var mainMsg = await ctx.EditResponseAsync(wbhk.AddEmbed(mainMenu.Build()).AddComponents(mainButton));

        var waitResult = await intr.WaitForButtonAsync(mainMsg,
            x => x.User.Id == ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOption),
            TimeSpan.FromMinutes(1));

        if (waitResult.TimedOut) return Result<DiscordEmbed>.FromError(new DiscordTimedOutError());

        var roleMenu = new RoleMenu
        {
            CreatorId = ctx.Member.Id,
            GuildId = ctx.Guild.Id,
            LastEditById = ctx.Member.Id,
            Name = req.Name,
            Text = req.Text
        };
        roleMenu.CustomSelectComponentId = $"role_menu_{ctx.Guild.Id}_{roleMenu.Id}";
        roleMenu.CustomButtonId = $"role_menu_button_{ctx.Guild.Id}_{roleMenu.Id}";

        while (true)
        {
            if (loopCount > 25) return resultEmbed.Build();

            var result = await AddNewOptionAsync(ctx, intr, resultEmbed, roleMenu);

            if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(result);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed.Build())
                .AddEmbed(mainMenu.Build())
                .AddComponents(mainButton)
                .AddComponents(GetMainFinalizeButton(ctx.Client)));

            var waitForFinalButtonTask = intr.WaitForButtonAsync(mainMsg,
                x => x.User.Id == ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuFinalize),
                TimeSpan.FromMinutes(10));

            var waitForNextOptionTask = intr.WaitForButtonAsync(mainMsg,
                x => x.User.Id == ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOption),
                TimeSpan.FromMinutes(1));

            var taskAggregate = await Task.WhenAny(new[] { waitForFinalButtonTask, waitForNextOptionTask });

            if (taskAggregate.Result.TimedOut)
                return Result<DiscordEmbed>.FromError(new DiscordTimedOutError());
            if (taskAggregate.Result.Result.Id == nameof(RoleMenuButton.RoleMenuFinalize))
                break;

            loopCount++;
        }

        foreach (var option in roleMenu.RoleMenuOptions ?? throw new ArgumentNullException())
        {
            DiscordRole role = await ctx.Guild.CreateRoleAsync(option.Name);
            option.RoleId = role.Id;
            option.CustomSelectOptionValueId = $"role_menu_option_{role.Id}";
            await Task.Delay(300);
        }

        await _roleMenuService.AddAsync(roleMenu, true);

        return resultEmbed.Build();
    }

    public async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> GetAsync(RoleMenuGetReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild? guild = null;
        if (req.GuildId.HasValue)
        {
            guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
        }
        else if (req.Id.HasValue)
        {
            var result = await _roleMenuService.GetAsync(req.Id.Value);
            if (!result.IsDefined()) return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(result);
            guild = await _discord.Client.GetGuildAsync(result.Entity.GuildId);
        }

        if (guild is null) throw new InvalidOperationException();

        DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await GetAsync(guild, requestingUser, req);
    }

    public async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> GetAsync(InteractionContext ctx,
        RoleMenuGetReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await GetAsync(ctx.Guild, ctx.Member, req);
    }

    public async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> SendAsync(RoleMenuSendReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild? guild = null;
        if (req.GuildId.HasValue)
        {
            guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
        }
        else if (req.Id.HasValue)
        {
            var result = await _roleMenuService.GetAsync(req.Id.Value);
            if (!result.IsDefined()) return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(result);
            guild = await _discord.Client.GetGuildAsync(result.Entity.GuildId);
        }

        if (guild is null) throw new InvalidOperationException();

        DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
        DiscordChannel target = guild.GetChannel(req.ChannelId);

        return await SendAsync(guild, requestingUser, target, req);
    }

    public async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> SendAsync(InteractionContext ctx,
        RoleMenuSendReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await SendAsync(ctx.Guild, ctx.Member, ctx.ResolvedChannelMentions[0], req);
    }

    public async Task<Result> HandleRoleMenuButtonAsync(ComponentInteractionCreateEventArgs args)
    {
        if (!long.TryParse(args.Id.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim(),
                out var parsedRoleMenuId)) return Result.FromError(new DiscordError());

        var res = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
            new RoleMenuByIdAndGuildWithOptionsSpec(parsedRoleMenuId, args.Guild.Id));

        if (!res.IsDefined(out var roleMenu)) return Result.FromError(new NotFoundError());

        var builder = new DiscordFollowupMessageBuilder().AsEphemeral(true);
        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(roleMenu.Guild?.EmbedHexColor));
        embed.WithAuthor("Lisbeth Role Menu");
        embed.WithDescription("Please select roles you'd like to get or deselect them to drop them!");
        builder.AddEmbed(embed.Build())
            .AddComponents(this.GetRoleMenuSelect(roleMenu, (DiscordMember)args.User));

        await args.Interaction.CreateFollowupMessageAsync(builder);

        return Result.FromSuccess();
    }

    public async Task<Result> HandleOptionSelectionAsync(ComponentInteractionCreateEventArgs args)
    {
        if (!long.TryParse(args.Id.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim(),
                out var parsedRoleMenuId)) return Result.FromError(new DiscordError());

        var res = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
            new RoleMenuByIdAndGuildWithOptionsSpec(parsedRoleMenuId, args.Guild.Id));

        if (!res.IsDefined(out var roleMenu)) return Result.FromError(new NotFoundError());

        if (!args.Guild.HasSelfPermissions(Permissions.ManageRoles)) return new DiscordError(
            "Bot doesn't have Manage Roles permission");

        try
        {
            var member = (DiscordMember)args.User;
            var selectedMenuIds = args.Values
                .Select(x => ulong.Parse(x.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim()))
                .ToList();
            var userRoleIds = member.Roles.Select(r => r.Id).ToList();

            var grantedRoles = new List<string?>();
            var revokedRoles = new List<string?>();
            var roleLists = new List<List<string?>> { grantedRoles, revokedRoles };

            foreach (var option in roleMenu.RoleMenuOptions ?? throw new InvalidOperationException("Role menu options were null"))
            {
                if (!args.Guild.RoleExists(option.RoleId, out var role)) continue;
                if (!args.Guild.IsRoleHierarchyValid(role)) continue;

                if (selectedMenuIds.Contains(option.RoleId) && !userRoleIds.Contains(role.Id))
                {
                    await member.GrantRoleAsync(role);
                    grantedRoles.Add(role.Name);
                }
                else if (!selectedMenuIds.Contains(option.RoleId) && userRoleIds.Contains(role.Id))
                {
                    await member.RevokeRoleAsync(role);
                    revokedRoles.Add(role.Name);
                }
            }

            var embed = new DiscordEmbedBuilder().WithAuthor("Lisbeth Role Menu")
                .WithFooter($"Member Id: {member.Id}")
                .WithColor(new DiscordColor(roleMenu.Guild?.EmbedHexColor));

            for (int i = 0; i < roleLists.Count; i++)
            {
                if (roleLists[i].Count == 0) continue;

                string joined = "";
                for (int j = 0; j < roleLists[i].Count; j++)
                {
                    var toAdd = (j is 0 ? "" : "\n") + roleLists[i][j];

                    if (joined.Length + toAdd.Length <= 256)
                    {
                        joined += toAdd;
                        continue;
                    }

                    if (256 - joined.Length >= 5) joined += "\n...";
                    break;
                }

                string fieldName = i switch
                {
                    0 => "Granted roles",
                    1 => "Revoked roles",
                    2 => "Available roles",
                    _ => "Default"
                };
                embed.AddField(fieldName, joined);
            }

            // wait and refresh member
            await Task.Delay(500);
            member = await args.Guild.GetMemberAsync(member.Id);

            await args.Interaction.EditFollowupMessageAsync(args.Message.Id, new DiscordWebhookBuilder()
                .AddComponents(this.GetRoleMenuSelect(roleMenu, member))
                .AddEmbed(embed.Build()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to grant/revoke role: {ex.GetFullMessage()}");
        }

        return Result.FromSuccess();
    }

    private async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> GetAsync(DiscordGuild guild,
        DiscordMember requestingUser,
        RoleMenuGetReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
        if (req is null) throw new ArgumentNullException(nameof(req));

        Result<RoleMenu> partial;
        if (requestingUser.IsBotOwner(_discord.Client))
        {
            if (req.Id.HasValue)
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByIdWithOptionsSpec(req.Id.Value));
            }
            else
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByNameWithOptionsSpec(req.Name));
            }
        }
        else
        {
            var guildResult =
                await _guildDataService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByIdSpec(guild.Id));
            if (!guildResult.IsDefined())
                return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(guildResult);

            if (requestingUser.Guild.Id != guild.Id)
                return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(
                    new DiscordNotAuthorizedError());

            if (req.Id.HasValue)
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByIdAndGuildWithOptionsSpec(req.Id.Value, guild.Id));
            }
            else
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByNameAndGuildWithOptionsSpec(req.Name, guild.Id));
            }
        }

        if (!partial.IsDefined())
            return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(partial);

        if (partial.Entity.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
            return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(
                new DisabledEntityError(nameof(partial.Entity)));

        var builder = new DiscordWebhookBuilder();


        //builder.AddComponents(this.GetRoleMenuSelect(partial.Entity).Entity);
        var button = new DiscordButtonComponent(ButtonStyle.Primary, partial.Entity.CustomButtonId,
            "Lets manage my roles!"/*, false, new DiscordComponentEmoji(DiscordEmoji.FromName("::"))*/);

        builder.AddComponents(button);

        if (partial.Entity.EmbedConfig is null)
            return (builder.WithContent(partial.Entity.Text ?? throw new ArgumentNullException()),
                partial.Entity.Text ?? throw new ArgumentNullException());

        var embed = _embedProvider.GetEmbedFromConfig(partial.Entity.EmbedConfig)
            .WithFooter("Click on the button below to manage your roles!").Build();
        return (builder.AddEmbed(embed), partial.Entity.Text ?? throw new ArgumentNullException());
    }

    private async Task<Result<(DiscordWebhookBuilder? Builder, string Text)>> SendAsync(DiscordGuild guild,
        DiscordMember requestingUser,
        DiscordChannel target, RoleMenuSendReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (req is null) throw new ArgumentNullException(nameof(req));

        Result<RoleMenu> partial;
        if (requestingUser.IsBotOwner(_discord.Client))
        {
            if (req.Id.HasValue)
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByIdWithOptionsSpec(req.Id.Value));
            }
            else
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByNameWithOptionsSpec(req.Name));
            }
        }
        else
        {
            var guildResult =
                await _guildDataService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByIdSpec(guild.Id));
            if (!guildResult.IsDefined())
                return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(guildResult);

            if (requestingUser.Guild.Id != guild.Id)
                return Result<(DiscordWebhookBuilder? Builder, string Text)>.FromError(
                    new DiscordNotAuthorizedError());

            if (req.Id.HasValue)
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByIdAndGuildWithOptionsSpec(req.Id.Value, guild.Id));
            }
            else
            {
                partial = await _roleMenuService.GetSingleBySpecAsync<RoleMenu>(
                    new RoleMenuByNameAndGuildWithOptionsSpec(req.Name, guild.Id));
            }
        }

        if (!partial.IsDefined()) return new NotFoundError("Role menu not found");
        if (partial.Entity.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
            return new DisabledEntityError("Role menu is disabled");

        var builder = new DiscordWebhookBuilder();

        var button = new DiscordButtonComponent(ButtonStyle.Primary, partial.Entity.CustomButtonId,
            "Lets manage my roles!"/*, false, new DiscordComponentEmoji(DiscordEmoji.FromName("::"))*/);

        builder.AddComponents(button);
        if (partial.Entity.EmbedConfig is null)
        {
            await target.SendMessageAsync(new DiscordMessageBuilder().WithContent(partial.Entity.Text).AddComponents(button));
            return (null, partial.Entity.Text ?? throw new ArgumentNullException());
        }

        var embed = _embedProvider.GetEmbedFromConfig(partial.Entity.EmbedConfig).Build();
        await target.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(button));
        return (builder.AddEmbed(embed), partial.Entity.Text ?? throw new ArgumentNullException());
    }

    private async Task<Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>> AddNewOptionAsync(
        InteractionContext ctx,
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
            return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(new DiscordTimedOutError());
        }

        if (string.IsNullOrWhiteSpace(waitResult.Result.Content.Trim()))
            return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("Response can't be empty"));

        string name = waitResult.Result.Content.GetStringBetween("@name@", "@endName@").Trim();
        string desc = waitResult.Result.Content.GetStringBetween("@desc@", "@endDesc@").Trim();
        string emoji = waitResult.Result.Content.GetStringBetween("@emoji@", "@endEmoji@").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(
                new ArgumentNullError(nameof(name)));

        if (roleMenu.RoleMenuOptions.AnyNullable(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)))
            return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("This menu already contains an option with same name"));

        RoleMenuOption newOption = new() { Name = name };
        if (!string.IsNullOrWhiteSpace(desc)) newOption.Description = desc;

        if (!string.IsNullOrWhiteSpace(emoji) && !DiscordEmoji.TryFromName(ctx.Client, emoji, true, out _) && !DiscordEmoji.TryFromUnicode(ctx.Client, emoji, out _))
            return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("Invalid emoji provided."));

        DiscordEmoji? parsedUnicodeEmoji = null;

        if (!string.IsNullOrWhiteSpace(emoji) &&
            (DiscordEmoji.TryFromName(ctx.Client, emoji, true, out var parsedNameEmoji) ||
             DiscordEmoji.TryFromUnicode(ctx.Client, emoji, out parsedUnicodeEmoji)))
        {
            if (parsedUnicodeEmoji is not null && parsedUnicodeEmoji.IsAnimated || parsedNameEmoji is not null && parsedNameEmoji.IsAnimated)
                return Result<(DiscordEmbedBuilder Embed, RoleMenu CurrentMenu)>.FromError(
                    new DiscordArgumentError("Animated emojis are not supported."));

            newOption.Emoji = emoji;
        }

        roleMenu.AddRoleMenuOption(newOption);

        currentResult.AddField(name,
            $"Additional settings (if any): {(string.IsNullOrWhiteSpace(emoji) ? "" : $"Emoji - {emoji}")}{(string.IsNullOrWhiteSpace(desc) ? "" : $", Description - {desc}")}");

        return (currentResult, roleMenu);
    }

    private static DiscordEmbed GetTimedOutEmbed(string idOrName, bool isFirst = false)
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
        return new DiscordButtonComponent(ButtonStyle.Primary, nameof(RoleMenuButton.RoleMenuFinalize),
            "Finalize", false,
            new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:")));
    }

    private DiscordSelectComponent GetRoleMenuSelect(RoleMenu roleMenu, DiscordMember? member = null)
    {
        List<DiscordSelectComponentOption> options = new();

        foreach (var option in roleMenu.RoleMenuOptions ?? throw new InvalidOperationException())
        {
            DiscordEmoji? emoji = null;

            if (!string.IsNullOrWhiteSpace(option.Emoji))
                DiscordEmoji.TryFromName(_discord.Client, option.Emoji, true, out emoji);
            if (!string.IsNullOrWhiteSpace(option.Emoji))
                DiscordEmoji.TryFromUnicode(_discord.Client, option.Emoji, out emoji);

            options.Add(new DiscordSelectComponentOption(option.Name, option.CustomSelectOptionValueId,
                option.Description, member is not null && member.HasRole(option.RoleId, out _), emoji is null ? null : new DiscordComponentEmoji(emoji)));
        }

        var select = new DiscordSelectComponent(roleMenu.CustomSelectComponentId, "Choose a role!", options, false, 0,
            roleMenu.RoleMenuOptions.Count);

        return select;
    }
}