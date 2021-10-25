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
using Lisbeth.Bot.Application.Validation;
using Lisbeth.Bot.Domain.DTOs.Request;

// ReSharper disable InconsistentNaming

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class TicketSlashCommands : ApplicationCommandModule
    {
        public IDiscordTicketService _discordTicketService { private get; set; }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("ticket", "A command that allows managing tickets")]
        public async Task TicketHandlerCommand(InteractionContext ctx,
            [Option("action", "Type of action to perform")]
            TicketActionType action,
            [Option("target", "A user or a role to add")]
            SnowflakeObject target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true));

            DiscordEmbed embed;
            switch (action)
            {
                case TicketActionType.Add:
                    var addReq = new TicketAddReqDto(null, null, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id, target.Id);
                    var addReqValidator = new TicketAddReqValidator(ctx.Client);
                    await addReqValidator.ValidateAndThrowAsync(addReq);
                    embed = await _discordTicketService.AddToTicketAsync(ctx, addReq);
                    break;
                case TicketActionType.Remove:
                    var removeReq = new TicketRemoveReqDto(null, null, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id, target.Id);
                    var removeReqValidator = new TicketRemoveReqValidator(ctx.Client);
                    await removeReqValidator.ValidateAndThrowAsync(removeReq);
                    embed = await _discordTicketService.RemoveFromTicketAsync(ctx, removeReq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}