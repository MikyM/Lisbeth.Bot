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

using MikyM.Common.DataAccessLayer.Specifications.Expressions;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Extensions;

public static class IncludeExtensions
{
    public static IQueryable<T> Include<T>(this IQueryable<T> source, IncludeExpressionInfo info)
    {
        _ = info ?? throw new ArgumentNullException(nameof(info));

        var queryExpr = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include",
            new[] {info.EntityType, info.PropertyType}, source.Expression, info.LambdaExpression);

        return source.Provider.CreateQuery<T>(queryExpr);
    }

    public static IQueryable<T> ThenInclude<T>(this IQueryable<T> source, IncludeExpressionInfo info)
    {
        _ = info ?? throw new ArgumentNullException(nameof(info));
        _ = info.PreviousPropertyType ?? throw new ArgumentNullException(nameof(info.PreviousPropertyType));

        var queryExpr = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "ThenInclude",
            new[] {info.EntityType, info.PreviousPropertyType, info.PropertyType}, source.Expression,
            info.LambdaExpression);

        return source.Provider.CreateQuery<T>(queryExpr);
    }
}