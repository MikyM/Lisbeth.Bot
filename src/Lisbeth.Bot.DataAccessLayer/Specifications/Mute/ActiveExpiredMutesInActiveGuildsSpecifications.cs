// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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


using MikyM.Common.EfCore.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.Mute;

public class ActiveExpiredMutesInActiveGuildsSpecifications : Specification<Domain.Entities.Mute>
{
    public ActiveExpiredMutesInActiveGuildsSpecifications()
    {
        Where(x => !x.IsDisabled);
        Where(x => !x.Guild!.IsDisabled);
        Where(x => x.AppliedUntil.ToUniversalTime() <= DateTime.UtcNow);
        OrderBy(x => x.Guild!.Id);
    }
}