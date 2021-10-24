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

using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Validation;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class BanApplicationCommands : ApplicationCommandModule
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
            [Option("length", "For how long should the user be banned")]
            string length = "perm",
            [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbed embed;
            ulong validId = user?.Id ?? (ulong) id;

            switch (actionType)
            {
                case BanActionType.Add:

                    DateTimeOffset? liftsOn = length.ToDateTimeOffsetDuration().FinalDateFromToday;

                    if (liftsOn is null)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

                    var banReq = new BanReqDto(validId, ctx.Guild.Id, ctx.User.Id, liftsOn.Value, reason);
                    var banReqValidator = new BanReqValidator(ctx.Client);
                    await banReqValidator.ValidateAndThrowAsync(banReq);

                    embed = await _discordBanService.BanAsync(ctx, banReq);
                    break;
                case BanActionType.Remove:
                    if (id == 0)
                        throw new ArgumentException("You must supply an Id of the user to unban.");

                    var banDisableReq = new BanDisableReqDto(validId, ctx.Guild.Id, ctx.User.Id);
                    var banDisableReqValidator = new BanDisableReqValidator(ctx.Client);
                    await banDisableReqValidator.ValidateAndThrowAsync(banDisableReq);

                    embed = await _discordBanService.UnbanAsync(ctx, banDisableReq);
                    break;
                case BanActionType.Get:
                    var banGetReq = new BanGetReqDto(ctx.User.Id, null, validId, ctx.Guild.Id);
                    var banGetReqValidator = new BanGetReqValidator(ctx.Client);
                    await banGetReqValidator.ValidateAndThrowAsync(banGetReq);
                    embed = await _discordBanService.GetSpecificUserGuildBanAsync(ctx, banGetReq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}