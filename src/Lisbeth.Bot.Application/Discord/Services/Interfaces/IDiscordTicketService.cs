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

using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTicketService
    {
        Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req);
        Task<DiscordMessageBuilder> CloseTicketAsync(DiscordInteraction intr, TicketCloseReqDto req);
        Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req);
        Task<DiscordMessageBuilder> OpenTicketAsync(DiscordInteraction intr, TicketOpenReqDto req);
        Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req);
        Task<DiscordMessageBuilder> ReopenTicketAsync(DiscordInteraction intr, TicketReopenReqDto req);
        Task<DiscordEmbed> AddToTicketAsync(TicketAddReqDto req);
        Task<DiscordEmbed> AddToTicketAsync(InteractionContext intr, TicketAddReqDto req);
        Task<DiscordEmbed> RemoveFromTicketAsync(TicketRemoveReqDto req);
        Task<DiscordEmbed> RemoveFromTicketAsync(InteractionContext intr, TicketRemoveReqDto req);
        Task CleanClosedTicketsAsync();
        Task CloseInactiveTicketsAsync();
    }
}