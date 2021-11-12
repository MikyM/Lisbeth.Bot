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
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Validation.Ban;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using MikyM.Common.Application.Results;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class BanApplicationCommands : ExtendedApplicationCommandModule
    {
        [UsedImplicitly] public IDiscordBanService? DiscordBanService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("ban", "A command that allows banning a user.")]
        public async Task BanCommand(InteractionContext ctx,
            [Option("action", "Action type")] BanActionType actionType,
            [Option("user", "User to ban")] DiscordUser? user = null, [Option("id", "User Id to ban")] string id = "",
            [Option("length", "For how long should the user be banned")]
            string length = "perm", [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            Result<DiscordEmbed> result;

            if (user is null && id == "") throw new ArgumentException("You must supply either a user or a user Id");

            var validId = user?.Id ?? ulong.Parse(id);

            switch (actionType)
            {
                case BanActionType.Add:

                    bool isValid = length.TryParseToDurationAndNextOccurrence(out var occurrence, out _);

                    if (!isValid)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

                    var banReq = new BanReqDto(validId, ctx.Guild.Id, ctx.User.Id, occurrence, reason);
                    var banReqValidator = new BanReqValidator(ctx.Client);
                    await banReqValidator.ValidateAndThrowAsync(banReq);

                    result = await this.DiscordBanService!.BanAsync(ctx, banReq);
                    break;
                case BanActionType.Remove:
                    if (id == "") throw new ArgumentException("You must supply an Id of the user to unban.");

                    var banDisableReq = new BanDisableReqDto(validId, ctx.Guild.Id, ctx.User.Id);
                    var banDisableReqValidator = new BanDisableReqValidator(ctx.Client);
                    await banDisableReqValidator.ValidateAndThrowAsync(banDisableReq);

                    result = await this.DiscordBanService!.UnbanAsync(ctx, banDisableReq);
                    break;
                case BanActionType.Get:
                    var banGetReq = new BanGetReqDto(ctx.User.Id, null, validId, ctx.Guild.Id);
                    var banGetReqValidator = new BanGetReqValidator(ctx.Client);
                    await banGetReqValidator.ValidateAndThrowAsync(banGetReq);
                    result = await this.DiscordBanService!.GetSpecificUserGuildBanAsync(ctx, banGetReq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            if (result.IsDefined())
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(result.Entity)
                    .AsEphemeral(true));
            else
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(base.GetUnsuccessfulResultEmbed(result, ctx.Client))
                    .AsEphemeral(true));
        }
    }
}