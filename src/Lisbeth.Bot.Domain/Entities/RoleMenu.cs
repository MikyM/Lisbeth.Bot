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

using System;
using System.Collections.Generic;

namespace Lisbeth.Bot.Domain.Entities;

public class RoleMenu : EmbedConfigEntity
{
    private readonly HashSet<RoleMenuOption> _roleMenuOptions = new();
    public string? Text { get; set; }
    public string? CustomSelectComponentId { get; set; }
    public string? CustomButtonId { get; set; }
    public IReadOnlyCollection<RoleMenuOption>? RoleMenuOptions => _roleMenuOptions;

    public Guild? Guild { get; set; }

    public void AddRoleMenuOption(RoleMenuOption roleMenuOption)
    {
        if (roleMenuOption is null) throw new ArgumentNullException(nameof(roleMenuOption));
        _roleMenuOptions.Add(roleMenuOption);
    }
}