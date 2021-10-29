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
using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.Domain.DTOs.Request;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class TicketEventsHandler : IDiscordMiscEventsSubscriber, IDiscordChannelEventsSubscriber
    {
        private readonly IAsyncExecutor _asyncExecutor;
        private readonly ILogger<TicketEventsHandler> _logger;

        public TicketEventsHandler(IAsyncExecutor asyncExecutor, ILogger<TicketEventsHandler> logger)
        {
            _asyncExecutor = asyncExecutor;
            _logger = logger;
        }

        public Task DiscordOnChannelCreated(DiscordClient sender, ChannelCreateEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task DiscordOnChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task DiscordOnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs args)
        {
            _ = _asyncExecutor.ExecuteAsync<ITicketService>(async x =>
                await x.CheckForDeletedTicketChannelAsync(args.Channel.Id, args.Guild.Id, sender.CurrentUser.Id));
            return Task.CompletedTask;
        }

        public Task DiscordOnDmChannelDeleted(DiscordClient sender, DmChannelDeleteEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task DiscordOnChannelPinsUpdated(DiscordClient sender, ChannelPinsUpdateEventArgs args)
        {
            return Task.CompletedTask;
        }

        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender,
            ComponentInteractionCreateEventArgs args)
        {
            switch (args.Id)
            {
                case nameof(TicketButton.TicketCloseButton):
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    var closeReq = new TicketCloseReqDto(null, null, args.Guild.Id, args.Channel.Id, args.User.Id);
                    _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                        await x.CloseTicketAsync(args.Interaction, closeReq));
                    break;
                case nameof(TicketButton.TicketOpenButton):
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().AsEphemeral(true));
                    var openReq = new TicketOpenReqDto {GuildId = args.Guild.Id, OwnerId = args.User.Id};
                    _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                        await x.OpenTicketAsync(args.Interaction, openReq));
                    break;
                case nameof(TicketSelect.TicketCloseMessageSelect):
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    switch (args.Values[0])
                    {
                        case nameof(TicketSelectValue.TicketReopenValue):
                            var req = new TicketReopenReqDto
                            {
                                GuildId = args.Guild.Id, ChannelId = args.Channel.Id, RequestedById = args.User.Id
                            };
                            _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                                await x.ReopenTicketAsync(args.Interaction, req));
                            break;
                        case nameof(TicketSelectValue.TicketTranscriptValue):
                            _ = _asyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x =>
                                await x.ExportToHtmlAsync(args.Interaction));
                            break;
                    }

                    break;
            }
        }

        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            //_logger.LogError(args.Exception.GetFullMessage());
            return Task.CompletedTask;
        }
    }
}