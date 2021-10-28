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

using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using System;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications
{
    public class ActiveExpiredMutePerGuildSpec : Specification<Mute>
    {
        public ActiveExpiredMutePerGuildSpec(ulong targetUserId, ulong guildId)
        {
            Where(x => !x.IsDisabled);
            Where(x => x.GuildId == guildId);
            Where(x => x.UserId == targetUserId);
            Where(x => x.AppliedUntil <= DateTime.UtcNow);
            Where(x => !x.Guild.IsDisabled);
        }
    }
}