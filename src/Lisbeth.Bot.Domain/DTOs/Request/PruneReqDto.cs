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

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class PruneReqDto
    {
        public PruneReqDto(int count, ulong? messageId = null, ulong? targetAuthorId = null, ulong? channelId = null,
            ulong? guildId = null, ulong? moderatorId = null)
        {
            Count = count;
            MessageId = messageId;
            TargetAuthorId = targetAuthorId;
            ChannelId = channelId;
            GuildId = guildId;
            ModeratorId = moderatorId;
        }

        public int Count { get; set; } = 100;
        public ulong? TargetAuthorId { get; set; }
        public ulong? MessageId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ModeratorId { get; set; }
    }
}