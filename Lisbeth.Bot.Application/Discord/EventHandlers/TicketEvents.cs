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
using DSharpPlus.EventArgs;
using MikyM.Discord.Events;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class TicketEvents : IDiscordMiscEventsSubscriber
    {
        private readonly IDiscordTicketService _discordTicketService;

        public TicketEvents(IDiscordTicketService discordTicketService)
        {
            _discordTicketService = discordTicketService;
        }

        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (!args.Channel.Name.StartsWith("ticket-"))
                return;
            await args.Interaction.CreateResponseAsync(InteractionResponseType.Pong);
            if (args.Id == "close-ticket-btn")
            {
                _ = Task.Run(async () =>
                {
                    var req = new TicketCloseReqDto(null, null, args.Interaction.GuildId, args.Interaction.ChannelId, args.Interaction.User.Id);
                    return await _discordTicketService.CloseTicketAsync(args.Interaction);
                });
            }

            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }


        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
