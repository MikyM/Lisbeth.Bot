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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Domain.DTOs.Request;
using System;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public partial class MuteApplicationCommands : ApplicationCommandModule
    {
        // ReSharper disable once InconsistentNaming
        public IDiscordMuteService _discordMuteService { private get; set; }
        public IDiscordMessageService _discordMessageService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("mute", "A command that allows mute actions.")]
        [UsedImplicitly]
        public async Task MuteCommand(InteractionContext ctx, [Option("action", "Action type")] MuteActionType actionType,
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")] string length = "",
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbed embed;

            switch (actionType)
            {
                case MuteActionType.Add:
                    DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;
                    if (liftsOn is null)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");
                    if (length is "") throw new ArgumentException($"Parameter {nameof(length)} can't be empty.");
                    embed = await _discordMuteService.MuteAsync(ctx, liftsOn.Value, reason);
                    break;
                case MuteActionType.Remove:
                    embed = await _discordMuteService.UnmuteAsync(ctx);
                    break;
                case MuteActionType.Get:
                    embed = await _discordMuteService.GetAsync(ctx);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}
