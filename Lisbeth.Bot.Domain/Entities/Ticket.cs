﻿// This file is part of Lisbeth.Bot project
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
    public class Ticket : DiscordAggregateRootEntity
    {
        public ulong ChannelId { get; set; }
        public ulong GuildSpecificId { get; set; }
        public DateTime? ReopenedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public ulong? ClosedBy { get; set; }
        public ulong? ReopenedBy { get; set; }
        public ulong MessageOpenId { get; set; }
        public ulong? MessageCloseId { get; set; }
        public ulong? MessageReopenId { get; set; }
        public bool IsPrivate { get; set; } = false;
    }
}
