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
    public class MuteReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime? AppliedUntil { get; set; }
        public ulong AppliedById { get; set; }
        public string Reason { get; set; }

        public MuteReqDto()
        {
        }
        public MuteReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil) : this(user, guild, appliedById, appliedUntil, null)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
        }
        public MuteReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil, string reason)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
            Reason = reason;
        }
    }
}
