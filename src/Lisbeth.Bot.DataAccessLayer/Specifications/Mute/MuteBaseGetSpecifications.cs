// This file is part of Lisbeth.Bot project
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

using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.Mute;

public class MuteBaseGetSpecifications : Specification<Domain.Entities.Mute>
{
    public MuteBaseGetSpecifications(long? id = null, ulong? userId = null, ulong? guildId = null,
        ulong? appliedById = null, DateTime? liftedOn = null, DateTime? appliedOn = null, ulong? liftedById = null,
        int limit = 1, bool? isDisabled = null)
    {
        if (id is not null)
            Where(x => x.Id == id.Value);
        if (userId is not null)
            Where(x => x.UserId == userId.Value);
        if (guildId is not null)
            Where(x => x.GuildId == guildId.Value);
        if (appliedById is not null)
            Where(x => x.AppliedById == appliedById.Value);
        if (liftedById is not null)
            Where(x => x.LiftedById == liftedById.Value);
        if (liftedOn is not null)
            Where(x => x.LiftedOn == liftedOn.Value);
        if (appliedOn is not null)
            Where(x => x.CreatedAt == appliedOn.Value);
        if (liftedById is not null)
            Where(x => x.LiftedById == liftedById.Value);

        if (isDisabled.HasValue) Where(x => x.IsDisabled == isDisabled.Value);

        OrderByDescending(x => x.CreatedAt);

        ApplyTake(limit);
    }
}