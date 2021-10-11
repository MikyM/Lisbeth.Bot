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

using Lisbeth.Bot.Domain.Entities.Base;
using System;

namespace Lisbeth.Bot.Domain.Entities
{
    public sealed class Prune : SnowflakeEntity
    {
        public ulong UserId { get; set; }
        public ulong ModeratorId { get; set; }
        public ulong TargetAuthorId { get; set; }
        public ulong ChannelId { get; set; }
        public string Messages { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
