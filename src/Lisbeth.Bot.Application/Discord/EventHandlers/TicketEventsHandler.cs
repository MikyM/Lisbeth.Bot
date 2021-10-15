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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Helpers;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class TicketEventsHandler : IDiscordMiscEventsSubscriber
    {
        private readonly IAsyncExecutor _asyncExecutor;

        public TicketEventsHandler(IAsyncExecutor asyncExecutor)
        {
            _asyncExecutor = asyncExecutor;
        }

        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender,
            ComponentInteractionCreateEventArgs args)
        {
            if (args.Id == "ticket_close_btn")
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                    await x.CloseTicketAsync(args.Interaction));
            }

            if (args.Id == "ticket_open_btn")
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                    await x.OpenTicketAsync(args.Interaction));
            }

            if (args.Id == "ticket_reopen_btn")
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                _ = _asyncExecutor.ExecuteAsync<IDiscordTicketService>(async x =>
                    await x.ReopenTicketAsync(args.Interaction));
            }

            if (args.Id == "ticket_save_trans_btn")
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                _ = _asyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x =>
                    await x.ExportToHtmlAsync(args.Interaction));
            }
        }

        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}