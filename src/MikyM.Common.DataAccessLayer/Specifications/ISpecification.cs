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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Builders;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    /// <summary>
    ///     Encapsulates query logic for <typeparamref name="T" />,
    ///     and projects the result into <typeparamref name="TResult" />.
    /// </summary>
    /// <typeparam name="T">The type being queried against.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ISpecification<T, TResult> : ISpecification<T> where T : class
    {
        /// <summary>
        ///     The transform function to apply to the <typeparamref name="T" /> element.
        /// </summary>
        Expression<Func<T, TResult>>? Selector { get; }

        /// <summary>
        ///     The transform function to apply to the result of the query encapsulated by the
        ///     <see cref="ISpecification{T, TResult}" />.
        /// </summary>
        new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; }

        new IEnumerable<TResult> Evaluate(IEnumerable<T> entities);
    }

    /// <summary>
    ///     Encapsulates query logic for <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type being queried against.</typeparam>
    public interface ISpecification<T> where T : class
    {
        /// <summary>
        ///     The collection of predicates to filter on.
        /// </summary>
        IEnumerable<Expression<Func<T, bool>>> WhereExpressions { get; }

        /// <summary>
        ///     The collection of predicates to group by.
        /// </summary>
       Expression<Func<T, bool>> GroupByExpression { get; }

        /// <summary>
        ///     The collections of functions used to determine the sorting (and subsequent sorting),
        ///     to apply to the result of the query encapsulated by the <see cref="ISpecification{T}" />.
        ///     <para>KeySelector, a function to extract a key from an element.</para>
        ///     <para>OrderType, whether to (subsequently) sort ascending or descending</para>
        /// </summary>
        IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; }

        /// <summary>
        ///     The collection of <see cref="IncludeExpressionInfo" />s describing each include expression.
        ///     This information is utilized to build Include/ThenInclude functions in the query.
        /// </summary>
        IEnumerable<IncludeExpressionInfo> IncludeExpressions { get; }

        /// <summary>
        ///     The collection of navigation properties, as strings, to include in the query.
        /// </summary>
        IEnumerable<string> IncludeStrings { get; }

        /// <summary>
        ///     The collection of 'SQL LIKE' operations, constructed by;
        ///     <list type="bullet">
        ///         <item>Selector, the property to apply the SQL LIKE against.</item>
        ///         <item>SearchTerm, the value to use for the SQL LIKE.</item>
        ///         <item>SearchGroup, the index used to group sets of Selectors and SearchTerms together.</item>
        ///     </list>
        /// </summary>
        IEnumerable<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)> SearchCriterias { get; }

        /// <summary>
        ///     The number of elements to return.
        /// </summary>
        int? Take { get; }

        /// <summary>
        ///     The number of elements to skip before returning the remaining elements.
        /// </summary>
        int? Skip { get; }

        /// <summary>
        ///     The transform function to apply to the result of the query encapsulated by the <see cref="ISpecification{T}" />.
        /// </summary>
        Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; }

        /// <summary>
        ///     Return whether or not the results should be cached.
        /// </summary>
        bool CacheEnabled { get; }
        string? CacheKey { get; }
        bool IsPagingEnabled { get; }
        /// <summary>
        ///     Returns whether or not the change tracker will track any of the entities
        ///     that are returned. When true, if the entity instances are modified, this will not be detected
        ///     by the change tracker.
        /// </summary>
        bool AsNoTracking { get; }

        bool AsSplitQuery { get; }
        bool AsNoTrackingWithIdentityResolution { get; }
        IEnumerable<T> Evaluate(IEnumerable<T> entities);
    }
}