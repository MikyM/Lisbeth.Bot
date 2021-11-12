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

using Microsoft.EntityFrameworkCore.Internal;

namespace MikyM.Common.DataAccessLayer.Helpers;

public static class DbContextExtensions
{
    public static TEntity? FindTracked<TEntity>(this DbContext context, params object[] keyValues)
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var key = entityType?.FindPrimaryKey();
        var stateManager = context.GetDependencies().StateManager;
        var entry = stateManager.TryGetEntry(key ?? throw new InvalidOperationException(), keyValues);
        return entry?.Entity as TEntity;
    }
}