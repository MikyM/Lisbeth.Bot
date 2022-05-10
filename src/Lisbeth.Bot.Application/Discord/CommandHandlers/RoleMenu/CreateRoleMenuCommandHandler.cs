// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using Microsoft.Extensions.Logging;
using MikyM.CommandHandlers;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class CreateRoleMenuCommandHandler : ICommandHandler<CreateRoleMenuCommand, DiscordMessageBuilder>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IRoleMenuDataService _roleMenuDataService;
    private readonly ILogger<CreateRoleMenuCommandHandler> _logger;
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _discordEmbedProvider;

    public CreateRoleMenuCommandHandler(IGuildDataService guildDataService, IRoleMenuDataService roleMenuDataService,
        ILogger<CreateRoleMenuCommandHandler> logger, IDiscordService discord, IDiscordEmbedProvider discordEmbedProvider)
    {
        _guildDataService = guildDataService;
        _roleMenuDataService = roleMenuDataService;
        _logger = logger;
        _discord = discord;
        _discordEmbedProvider = discordEmbedProvider;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(CreateRoleMenuCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx.User as DiscordMember ??
                              await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.User);

        if (!requestingUser.IsAdmin()) return new DiscordNotAuthorizedError();

        var guildResult = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(command.Dto.GuildId));
        if (!guildResult.IsDefined()) return Result<DiscordMessageBuilder>.FromError(guildResult);

        var count = await _roleMenuDataService.LongCountAsync(
            new RoleMenuByNameAndGuildWithOptionsSpec(command.Dto.Name ?? string.Empty, command.Dto.GuildId));

        if (!count.IsDefined(out var countRes) || countRes >= 1)
            return new SameEntityNamePerGuildError("Role menu", command.Dto.Name ?? "null");

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
        var mainMsg = await command.Ctx.EditResponseAsync(wbhk.AddEmbed(mainMenu.Build()).AddComponents(mainButton));

        var waitResult = await intr.WaitForButtonAsync(mainMsg,
            x => x.User.Id == command.Ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOption),
            TimeSpan.FromMinutes(1));

        if (waitResult.TimedOut) return new DiscordTimedOutError();

        var roleMenu = new Domain.Entities.RoleMenu
        {
            CreatorId = requestingUser.Id,
            GuildId = guild.Id,
            LastEditById = requestingUser.Id,
            Name = command.Dto.Name ?? string.Empty,
            Text = command.Dto.Text
        };
        roleMenu.CustomSelectComponentId = $"role_menu_{guild.Id}_{roleMenu.Id}";
        roleMenu.CustomButtonId = $"role_menu_button_{guild.Id}_{roleMenu.Id}";

        while (true)
        {
            if (loopCount > 25) 
                return new DiscordMessageBuilder().AddEmbed(resultEmbed);

            var result = await AddNewOptionAsync(command.Ctx, intr, resultEmbed, roleMenu);

            if (!result.IsDefined()) return Result<DiscordMessageBuilder>.FromError(result);

            await command.Ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed.Build())
                .AddEmbed(mainMenu.Build())
                .AddComponents(mainButton)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, nameof(RoleMenuButton.RoleMenuFinalize),
                    "Finalize", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(command.Ctx.Client, ":white_check_mark:")))));

            var waitForFinalButtonTask = intr.WaitForButtonAsync(mainMsg,
                x => x.User.Id == command.Ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuFinalize),
                TimeSpan.FromMinutes(10));

            var waitForNextOptionTask = intr.WaitForButtonAsync(mainMsg,
                x => x.User.Id == command.Ctx.Member.Id && x.Id == nameof(RoleMenuButton.RoleMenuAddOption),
                TimeSpan.FromMinutes(1));

            var taskAggregate = await Task.WhenAny(new[] { waitForFinalButtonTask, waitForNextOptionTask });

            if (taskAggregate.Result.TimedOut)
                return new DiscordTimedOutError();
            if (taskAggregate.Result.Result.Id == nameof(RoleMenuButton.RoleMenuFinalize))
                break;

            loopCount++;
        }

        foreach (var option in roleMenu.RoleMenuOptions ?? throw new ArgumentNullException())
        {
            DiscordRole role = await guild.CreateRoleAsync(option.Name);
            option.RoleId = role.Id;
            option.CustomSelectOptionValueId = $"role_menu_option_{role.Id}";
            await Task.Delay(300);
        }

        await _roleMenuDataService.AddAsync(roleMenu, true);

        return new DiscordMessageBuilder().AddEmbed(resultEmbed);
    }

    private async Task<Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>> AddNewOptionAsync(
    InteractionContext ctx,
    InteractivityExtension intr, DiscordEmbedBuilder currentResult, Domain.Entities.RoleMenu roleMenu)
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
                new DiscordWebhookBuilder().AddEmbed(_discordEmbedProvider.GetActionTimedOutEmbed()));
            return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(new DiscordTimedOutError());
        }

        if (string.IsNullOrWhiteSpace(waitResult.Result.Content.Trim()))
            return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("Response can't be empty"));

        string name = waitResult.Result.Content.GetStringBetween("@name@", "@endName@").Trim();
        string desc = waitResult.Result.Content.GetStringBetween("@desc@", "@endDesc@").Trim();
        string emoji = waitResult.Result.Content.GetStringBetween("@emoji@", "@endEmoji@").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(
                new ArgumentNullError(nameof(name)));

        if (roleMenu.RoleMenuOptions.AnyNullable(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)))
            return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("This menu already contains an option with same name"));

        RoleMenuOption newOption = new() { Name = name };
        if (!string.IsNullOrWhiteSpace(desc)) newOption.Description = desc;

        if (!string.IsNullOrWhiteSpace(emoji) && !DiscordEmoji.TryFromName(ctx.Client, emoji, true, out _) && !DiscordEmoji.TryFromUnicode(ctx.Client, emoji, out _))
            return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(
                new DiscordArgumentError("Invalid emoji provided."));

        DiscordEmoji? parsedUnicodeEmoji = null;

        if (!string.IsNullOrWhiteSpace(emoji) &&
            (DiscordEmoji.TryFromName(ctx.Client, emoji, true, out var parsedNameEmoji) ||
             DiscordEmoji.TryFromUnicode(ctx.Client, emoji, out parsedUnicodeEmoji)))
        {
            if (parsedUnicodeEmoji is not null && parsedUnicodeEmoji.IsAnimated || parsedNameEmoji is not null && parsedNameEmoji.IsAnimated)
                return Result<(DiscordEmbedBuilder Embed, Domain.Entities.RoleMenu CurrentMenu)>.FromError(
                    new DiscordArgumentError("Animated emojis are not supported."));

            newOption.Emoji = emoji;
        }

        roleMenu.AddRoleMenuOption(newOption);

        currentResult.AddField(name,
            $"Additional settings (if any): {(string.IsNullOrWhiteSpace(emoji) ? "" : $"Emoji - {emoji}")}{(string.IsNullOrWhiteSpace(desc) ? "" : $", Description - {desc}")}");

        return (currentResult, roleMenu);
    }
}
