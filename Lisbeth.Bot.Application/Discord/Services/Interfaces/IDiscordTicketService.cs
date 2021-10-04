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
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTicketService
    {
        Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
        Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
        Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
    }
}
