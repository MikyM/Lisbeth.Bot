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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Domain.DTOs.Request;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    // Menus for prunes
    public partial class PruneApplicationCommands
    {
        #region user

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Prune last 10 messages")]
        public async Task PruneLastTenUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(10, null, ctx.TargetUser.Id, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion

        #region message

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune last 10")]
        public async Task PruneLastTenFromThisMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(10, null, null, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune")]
        public async Task PruneUntilThisMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(0, ctx.TargetMessage.Id, null, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune by author")]
        public async Task PruneUntilThisByThisAuthorMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(0, ctx.TargetMessage.Id, ctx.TargetMessage.Author.Id, ctx.Channel.Id,
                ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion
    }
}