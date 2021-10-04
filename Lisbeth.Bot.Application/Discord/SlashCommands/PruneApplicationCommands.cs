﻿// This file is part of Lisbeth.Bot project
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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public partial class PruneApplicationCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public IDiscordMessageService _discordMessageService { private get; set; }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [SlashCommand("prune", "A command that allows banning a user.")]
        public async Task PruneCommand(InteractionContext ctx,
            [Option("count", "Number of messages to prune")] long count,
            [Option("user", "User to target prune at")] DiscordUser user = null,
            [Option("id", "Message id to prune to")] long id = 0,
            [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (count > 99) count = 99;

            DiscordEmbed embed;
            switch (user)
            {
                case null:
                    switch (id)
                    {
                        case 0:
                            var reqNoUsNoMsgId = new PruneReqDto((int)count + 1);
                            embed = await _discordMessageService.PruneAsync(reqNoUsNoMsgId, 0, ctx);
                            break;
                        default:
                            var reqNoUsWithMsgId = new PruneReqDto((int)count, (ulong)id);
                            embed = await _discordMessageService.PruneAsync(reqNoUsWithMsgId, 0, ctx);
                            break;
                    }
                    break;
                default:
                    switch (id)
                    {
                        case 0:
                            var reqWithUsNoMsgId = new PruneReqDto((int)count, null, user.Id);
                            embed = await _discordMessageService.PruneAsync(reqWithUsNoMsgId, 0, ctx);
                            break;
                        default:
                            var reqWithUsWithMsgId = new PruneReqDto((int)count, (ulong)id, user.Id);
                            embed = await _discordMessageService.PruneAsync(reqWithUsWithMsgId, 0, ctx);
                            break;
                    }
                    break;
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

    }
}