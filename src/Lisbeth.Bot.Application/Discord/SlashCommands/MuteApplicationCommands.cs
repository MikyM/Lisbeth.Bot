﻿// This file is part of Lisbeth.Bot project
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
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Validation;
using Lisbeth.Bot.Domain.DTOs.Request;

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
        public async Task MuteCommand(InteractionContext ctx,
            [Option("action", "Action type")] MuteActionType actionType,
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")]
            string length = "",
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbed embed;

            switch (actionType)
            {
                case MuteActionType.Add:
                    DateTime? liftsOn = length.ToDateTimeOffsetDuration().FinalDateFromToday;
                    if (liftsOn is null)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");
                    if (length is "") throw new ArgumentException($"Parameter {nameof(length)} can't be empty.");

                    var muteReq = new MuteReqDto(user.Id, ctx.Guild.Id, ctx.User.Id, liftsOn.Value, reason);
                    var muteReqValidator = new MuteReqValidator(ctx.Client);
                    await muteReqValidator.ValidateAndThrowAsync(muteReq);

                    embed = await _discordMuteService.MuteAsync(ctx, muteReq);
                    break;
                case MuteActionType.Remove:
                    var muteDisableReq = new MuteDisableReqDto(user.Id, ctx.Guild.Id, ctx.User.Id);
                    var muteDisableReqValidator = new MuteDisableReqValidator(ctx.Client);
                    await muteDisableReqValidator.ValidateAndThrowAsync(muteDisableReq);

                    embed = await _discordMuteService.UnmuteAsync(ctx, muteDisableReq);
                    break;
                case MuteActionType.Get:
                    var muteGetReq = new MuteGetReqDto(ctx.User.Id, null, user.Id, ctx.Guild.Id);
                    var muteGetReqValidator = new MuteGetReqValidator(ctx.Client);
                    await muteGetReqValidator.ValidateAndThrowAsync(muteGetReq);

                    embed = await _discordMuteService.GetSpecificUserGuildMuteAsync(ctx, muteGetReq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}