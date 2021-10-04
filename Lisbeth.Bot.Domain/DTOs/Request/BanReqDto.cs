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

using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class BanReqDto
    {
        public long Id { get; set; }
        public ulong TargetUserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime AppliedUntil { get; set; }
        public ulong AppliedOnBehalfOfId { get; set; }
        public string Reason { get; set; }

        public BanReqDto()
        {
        }
        public BanReqDto(ulong targetUserId, ulong guildId, ulong appliedOnBehalfOfId, DateTime appliedUntil) : this(targetUserId, guildId, appliedOnBehalfOfId, appliedUntil, null)
        {
            TargetUserId = targetUserId;
            GuildId = guildId;
            AppliedUntil = appliedUntil;
            AppliedOnBehalfOfId = appliedOnBehalfOfId;
        }
        public BanReqDto(ulong targetUserId, ulong guildId, ulong appliedOnBehalfOfId, DateTime appliedUntil, string reason)
        {
            TargetUserId = targetUserId;
            GuildId = guildId;
            AppliedUntil = appliedUntil;
            AppliedOnBehalfOfId = appliedOnBehalfOfId;
            Reason = reason;
        }
    }
}
