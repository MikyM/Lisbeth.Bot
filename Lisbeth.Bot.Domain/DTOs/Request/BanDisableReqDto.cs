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

using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class BanDisableReqDto
    {
        public long? Id { get; set; }
        public ulong? TargetUserId { get; set; }
        public ulong? GuildId { get; set; }
        public DateTime LiftedOn { get; set; } = DateTime.UtcNow;
        public ulong RequestedOnBehalfOfId { get; set; }

        public BanDisableReqDto()
        {
        }

        public BanDisableReqDto(ulong? targetUserId, ulong? guildId, ulong requestedOnBehalfOfId)
        {
            TargetUserId = targetUserId;
            GuildId = guildId;
            RequestedOnBehalfOfId = requestedOnBehalfOfId;
        }
    }
}