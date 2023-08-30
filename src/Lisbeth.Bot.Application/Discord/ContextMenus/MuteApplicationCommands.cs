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

using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Validation.Mute;
using Lisbeth.Bot.Application.Validation.Prune;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands;

// menus for mutes
public partial class MuteApplicationCommands
{
    #region user menus

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [ContextMenu(ApplicationCommandType.UserContextMenu, "Mute user", false)]
    public async Task MuteUserMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var muteReq = new MuteApplyReqDto(ctx.TargetUser.Id, ctx.Guild.Id, ctx.User.Id, DateTime.MaxValue,
            "No reason provided - muted via user context menu");
        var muteReqValidator = new MuteReqValidator(ctx.Client);
        await muteReqValidator.ValidateAndThrowAsync(muteReq);

        var result = await _applyHandler.HandleAsync(new ApplyMuteCommand(muteReq, ctx));

        if (result.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [ContextMenu(ApplicationCommandType.UserContextMenu, "Unmute user", false)]
    public async Task UnmuteUserMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var muteDisableReq = new MuteRevokeReqDto(ctx.TargetUser.Id, ctx.Guild.Id, ctx.User.Id);
        var muteDisableReqValidator = new MuteDisableReqValidator(ctx.Client);
        await muteDisableReqValidator.ValidateAndThrowAsync(muteDisableReq);

        var result = await _revokeHandler.HandleAsync(new RevokeMuteCommand(muteDisableReq, ctx));

        if (result.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [ContextMenu(ApplicationCommandType.UserContextMenu, "Get mute info", false)]
    public async Task GetMuteUserMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var muteGetReq = new MuteGetReqDto(ctx.User.Id, ctx.Guild.Id, null, ctx.TargetUser.Id);
        var muteGetReqValidator = new MuteGetReqValidator(ctx.Client);
        await muteGetReqValidator.ValidateAndThrowAsync(muteGetReq);

        var result = await _getHandler.HandleAsync(new GetMuteInfoCommand(muteGetReq, ctx));

        if (result.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    #endregion

    #region message menus

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author", false)]
    public async Task MuteAuthorMessageMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var muteReq = new MuteApplyReqDto(ctx.TargetMessage.Author.Id, ctx.Guild.Id, ctx.User.Id, DateTime.MaxValue,
            "No reason provided - muted via user context menu");
        var muteReqValidator = new MuteReqValidator(ctx.Client);
        await muteReqValidator.ValidateAndThrowAsync(muteReq);

        var result = await _applyHandler.HandleAsync(new ApplyMuteCommand(muteReq, ctx));

        if (result.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author and prune", false)]
    public async Task MuteAuthorWithWipeMessageMenu(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var muteReq = new MuteApplyReqDto(ctx.TargetMessage.Author.Id, ctx.Guild.Id, ctx.User.Id, DateTime.MaxValue,
            "No reason provided - muted via user context menu");
        var muteReqValidator = new MuteReqValidator(ctx.Client);
        await muteReqValidator.ValidateAndThrowAsync(muteReq);

        var pruneReq = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id, null, null,
            ctx.TargetMessage.Author.Id);
        var pruneReqValidator = new PruneReqValidator(ctx.Client);
        await pruneReqValidator.ValidateAndThrowAsync(pruneReq);

        var result = await _applyHandler.HandleAsync(new ApplyMuteCommand(muteReq, ctx));
        var pruneResult = await _discordMessageService.PruneAsync(ctx, pruneReq);

        if (result.IsDefined() && pruneResult.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    #endregion
}
