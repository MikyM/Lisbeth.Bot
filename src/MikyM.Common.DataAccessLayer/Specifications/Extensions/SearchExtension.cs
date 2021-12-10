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

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Extensions;

public static class SearchExtension
{
    /// <summary>
    ///     Filters <paramref name="source" /> by applying an 'SQL LIKE' operation to it.
    /// </summary>
    /// <typeparam name="T">The type being queried against.</typeparam>
    /// <param name="source">The sequence of <typeparamref name="T" /></param>
    /// <param name="criterias">
    ///     <list type="bullet">
    ///         <item>Selector, the property to apply the SQL LIKE against.</item>
    ///         <item>SearchTerm, the value to use for the SQL LIKE.</item>
    ///     </list>
    /// </param>
    /// <returns></returns>
    public static IQueryable<T> Search<T>(this IQueryable<T> source,
        IEnumerable<(Expression<Func<T, string>> selector, string searchTerm)> criterias)
    {
        Expression? expr = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var (selector, searchTerm) in criterias)
        {
            if (string.IsNullOrEmpty(searchTerm))
                continue;

            var functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);
            var like = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { functions.Type, typeof(string), typeof(string) });

            var propertySelector = ParameterReplacerVisitor.Replace(selector, selector.Parameters[0], parameter);

            var likeExpression = Expression.Call(
                null,
                like!,
                functions,
                (propertySelector as LambdaExpression)?.Body!,
                Expression.Constant(searchTerm));

            expr = expr == null ? (Expression)likeExpression : Expression.OrElse(expr, likeExpression);
        }

        return expr == null
            ? source
            : source.Where(Expression.Lambda<Func<T, bool>>(expr, parameter));
    }
}
