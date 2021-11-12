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
        var parameter = Expression.Parameter(typeof(T), "x");

        Expression? expr =
            (from criteria in criterias
                where criteria.selector is not null && !string.IsNullOrEmpty(criteria.searchTerm)
                let functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions)) ?? throw new InvalidOperationException())
                let like =
                    typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
                        new[] {functions.Type, typeof(string), typeof(string)})
                let propertySelector =
                    ParameterReplacerVisitor.Replace(criteria.selector, criteria.selector.Parameters[0], parameter)
                select Expression.Call(null, like, functions, (propertySelector as LambdaExpression)?.Body ?? throw new InvalidOperationException(),
                    Expression.Constant(criteria.searchTerm))).Aggregate<MethodCallExpression, Expression?>(null,
                (current, likeExpression) =>
                    current is null ? likeExpression : Expression.OrElse(current, likeExpression));

        return expr is null ? source : source.Where(Expression.Lambda<Func<T, bool>>(expr, parameter));
    }
}