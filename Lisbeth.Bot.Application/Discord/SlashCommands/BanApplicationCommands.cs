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
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Npgsql;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public partial class BanApplicationCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public IDiscordBanService _discordBanService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("ban", "A command that allows banning a user.")]
        public async Task BanCommand(InteractionContext ctx,
            [Option("action", "Action type")] BanActionType actionType,
            [Option("user", "User to ban")] DiscordUser user = null,
            [Option("id", "User Id to ban")] long id = 0,
            [Option("length", "For how long should the user be banned")] string length = "perm",
            [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbed embed;

            switch (actionType)
            {
                case BanActionType.Add:
                    if(user is null && id == 0)
                        throw new ArgumentException($"You must supply either a user or an Id.");

                    DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;
                    if (liftsOn is null)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

                    ulong validId = user?.Id ?? (ulong)id;

                    var newReq = new BanReqDto(validId, ctx.Guild.Id, ctx.Member.Id, liftsOn, reason);
                    embed = await _discordBanService.BanAsync(newReq, 0, ctx);
                    break;
                case BanActionType.Remove:
                    if (id == 0)
                        throw new ArgumentException($"You must supply an Id of the user to unban.");
                    var disReq = new BanDisableReqDto((ulong)id, ctx.Guild.Id, ctx.Member.Id);
                    embed = await _discordBanService.UnbanAsync(disReq, 0, ctx);
                    break;
                case BanActionType.Get:
                    if (id == 0)
                        throw new ArgumentException($"You must supply an Id of the user to unban.");
                    var getReq = new BanGetReqDto(null, (ulong)id, ctx.Guild.Id);
                    embed = await _discordBanService.GetAsync(getReq, 0, ctx);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}
