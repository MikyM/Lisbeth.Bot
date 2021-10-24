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

using Lisbeth.Bot.Domain.Entities.Base;
using System.Collections.Generic;

namespace Lisbeth.Bot.Domain.Entities
{
    public class RoleMenu : SnowflakeEntity
    {
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public EmbedConfig EmbedConfig { get; set; }
        public string Text { get; set; }
        public long EmbedConfigId { get; set; }
        public List<RoleEmojiMapping> RoleEmojiMapping { get; set; }

        public Guild Guild { get; set; }
    }
}