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
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands;

// Menus for prunes
public partial class PruneApplicationCommands
{
    #region user

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [ContextMenu(ApplicationCommandType.UserContextMenu, "Prune last 10 messages", false)]
    public async Task PruneLastTenUserMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

        var req = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, 10);

        var result = await _discordMessageService.PruneAsync(ctx, req);

        if (!result.IsDefined(out var embed))
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral());
    }

    #endregion

    #region message

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune last 10", false)]
    public async Task PruneLastTenFromThisMessageMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

        var req = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, 10);

        var result = await _discordMessageService.PruneAsync(ctx, req);

        if (!result.IsDefined(out var embed))
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune until this", false)]
    public async Task PruneUntilThisMessageMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

        var req = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, null, ctx.TargetMessage.Id);

        var result = await _discordMessageService.PruneAsync(ctx, req);

        if (!result.IsDefined(out var embed))
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune by author", false)]
    public async Task PruneUntilThisByThisAuthorMessageMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

        var req = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, null, null, ctx.TargetMessage.Author.Id);

        var result = await _discordMessageService.PruneAsync(ctx, req);

        if (!result.IsDefined(out var embed))
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral());
    }

    #endregion
}