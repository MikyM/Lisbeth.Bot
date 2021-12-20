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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.Application.Discord.EventHandlers.Base;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class TicketEventsHandler : BaseEventHandler, IDiscordMiscEventsSubscriber, IDiscordChannelEventsSubscriber
{
    public TicketEventsHandler(IAsyncExecutor asyncExecutor) : base(asyncExecutor){}


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
        _ = AsyncExecutor.ExecuteAsync<ITicketDataService>(async x =>
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
            case nameof(TicketButton.TicketClose):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AsEphemeral(true));
                var handleCloseButtonReq = new CloseTicketCommand(args.Interaction);
                _ = AsyncExecutor.ExecuteAsync<ICommandHandler<CloseTicketCommand>>(x =>
                    x.HandleAsync(handleCloseButtonReq));
                break;
            case nameof(TicketButton.TicketOpen):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AsEphemeral(true));
                var openReq = new TicketOpenReqDto { GuildId = args.Guild.Id, RequestedOnBehalfOfId = args.User.Id };
                _ = AsyncExecutor.ExecuteAsync<ITicketQueueService>(x => x.EnqueueAsync(openReq, args.Interaction));
                break;
            case nameof(TicketSelect.TicketCloseMessageSelect):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                switch (args.Values[0])
                {
                    case nameof(TicketSelectValue.TicketReopenValue):
                        var req = new TicketReopenReqDto
                        {
                            GuildId = args.Guild.Id, ChannelId = args.Channel.Id,
                            RequestedOnBehalfOfId = args.User.Id
                        };
                        _ = AsyncExecutor.ExecuteAsync<ICommandHandler<ReopenTicketCommand, DiscordMessageBuilder>>(async x =>
                            await x.HandleAsync(new ReopenTicketCommand(req, args.Interaction)));
                        break;
                    case nameof(TicketSelectValue.TicketTranscriptValue):
                        _ = AsyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x =>
                            await x.ExportToHtmlAsync(args.Interaction));
                        break;
                    case nameof(TicketSelectValue.TicketDeleteValue):
                        _ = AsyncExecutor.ExecuteAsync<ICommandHandler<DeleteTicketCommand>>(async x =>
                            await x.HandleAsync(new DeleteTicketCommand(args.Interaction)));
                        break;
                }
                break;
            case nameof(TicketButton.TicketCloseConfirm):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                var closeReq = new TicketCloseReqDto(null, args.Guild.Id, args.Channel.Id, args.User.Id);
                _ = AsyncExecutor.ExecuteAsync<ICommandHandler<ConfirmCloseTicketCommand>>(async x =>
                    await x.HandleAsync(new ConfirmCloseTicketCommand(closeReq, args.Interaction)));
                break;
            case nameof(TicketButton.TicketCloseReject):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AsEphemeral(true));
                var rejectReq = new RejectCloseTicketCommand(args.Interaction, args.Message);
                _ = AsyncExecutor.ExecuteAsync<ICommandHandler<RejectCloseTicketCommand>> (x =>
                    x.HandleAsync(rejectReq));
                break;
        }
    }

    public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
    {
        //_logger.LogError(args.Exception.GetFullMessage());
        return Task.CompletedTask;
    }
}