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

using System.Collections.Generic;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class EmbedConfig : SnowflakeEntity
    {
        public string Author { get; set; }
        public string Footer { get; set; }
        public string ImageUrl { get; set; }
        public string FooterImageUrl { get; set; }
        public string AuthorImageUrl { get; set; }
        public string Description { get; set; }
        public List<DiscordField> Fields { get; set; }

        public Reminder Reminder { get; set; }
        public RecurringReminder RecurringReminder { get; set; }
        public Tag Tag { get; set; }
        public RoleMenu RoleMenu { get; set; }
    }
}