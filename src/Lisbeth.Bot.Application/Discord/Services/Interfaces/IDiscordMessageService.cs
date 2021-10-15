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
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordMessageService
    {
        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, InteractionContext ctx = null,
            bool isSingleMessageDelete = false);

        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null,
            bool isSingleMessageDelete = false);

        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, DiscordChannel channel = null,
            DiscordGuild guild = null, DiscordUser moderator = null, DiscordUser author = null,
            DiscordMessage message = null, bool isSingleMessageDelete = false, ulong idToSkip = 0);

        Task LogMessageUpdatedEventAsync(MessageUpdateEventArgs args);
        Task LogMessageDeletedEventAsync(MessageDeleteEventArgs args);
        Task LogMessageBulkDeletedEventAsync(MessageBulkDeleteEventArgs args);
    }
}