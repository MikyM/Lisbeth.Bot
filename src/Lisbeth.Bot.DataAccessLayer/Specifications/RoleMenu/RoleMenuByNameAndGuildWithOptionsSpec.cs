﻿// This file is part of Lisbeth.Bot project
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

using DataExplorer.EfCore.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;

public class RoleMenuByNameAndGuildWithOptionsSpec : Specification<Domain.Entities.RoleMenu>
{
    public RoleMenuByNameAndGuildWithOptionsSpec(string name, ulong guildId)
    {
        Where(x => x.GuildId == guildId);
        Where(x => x.Name == name);
        Include(x => x.RoleMenuOptions);
        Include(x => x.EmbedConfig);
    }
}