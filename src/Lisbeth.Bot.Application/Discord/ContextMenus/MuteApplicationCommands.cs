// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    // menus for mutes
    public partial class MuteApplicationCommands
    {
        #region user menus

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Mute user")]
        public async Task MuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = await _discordMuteService.MuteAsync(ctx, DateTime.MaxValue,
                "No reason provided - muted via user context menu");

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Unmute user")]
        public async Task UnmuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = await _discordMuteService.UnmuteAsync(ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Get mute info")]
        public async Task GetMuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = await _discordMuteService.GetAsync(ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion

        #region message menus

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author")]
        public async Task MuteAuthorMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = await _discordMuteService.MuteAsync(ctx, DateTime.MaxValue,
                "No reason provided - muted via message context menu");

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author and prune")]
        public async Task MuteAuthorWithWipeMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = await _discordMuteService.MuteAsync(ctx, DateTime.MaxValue,
                "No reason provided - muted via message context menu");

            //await _discordMessageService.PruneAsync()

            var msgs = await ctx.Channel.GetMessagesAsync();

            var msgsToDel = msgs.Where(x => x.Author.Id == ctx.TargetMessage.Author.Id)
                .OrderByDescending(x => x.Timestamp).Take(10);

            await ctx.Channel.DeleteMessagesAsync(msgsToDel);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion
    }
}