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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.Results;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTicketService
    {
        Task<Result<DiscordMessageBuilder>> CloseTicketAsync(TicketCloseReqDto req);
        Task<Result<DiscordMessageBuilder>> CloseTicketAsync(DiscordInteraction intr, TicketCloseReqDto req);
        Task<Result<DiscordMessageBuilder>> OpenTicketAsync(TicketOpenReqDto req);
        Task<Result<DiscordMessageBuilder>> OpenTicketAsync(DiscordInteraction intr, TicketOpenReqDto req);
        Task<Result<DiscordMessageBuilder>> ReopenTicketAsync(TicketReopenReqDto req);
        Task<Result<DiscordMessageBuilder>> ReopenTicketAsync(DiscordInteraction intr, TicketReopenReqDto req);
        Task<Result<DiscordEmbed>> AddToTicketAsync(TicketAddReqDto req);
        Task<Result<DiscordEmbed>> AddToTicketAsync(InteractionContext intr, TicketAddReqDto req);
        Task<Result<DiscordEmbed>> RemoveFromTicketAsync(TicketRemoveReqDto req);
        Task<Result<DiscordEmbed>> RemoveFromTicketAsync(InteractionContext intr, TicketRemoveReqDto req);
        Task<Result> CleanClosedTicketsAsync();
        Task<Result> CloseInactiveTicketsAsync();
        Task<Result<DiscordMessageBuilder>> GetTicketCenterEmbedAsync(InteractionContext ctx);
    }
}