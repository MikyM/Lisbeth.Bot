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

using System.Linq;
using MikyM.Common.DataAccessLayer.Filters;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators
{
    /// <summary>
    ///     Evaluates the logic encapsulated by an <see cref="ISpecification{T}" />.
    /// </summary>
    public interface ISpecificationEvaluator
    {
        /// <summary>
        ///     Applies the logic encapsulated by <paramref name="specification" /> to given <paramref name="inputQuery" />,
        ///     and projects the result into <typeparamref name="TResult" />.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="inputQuery">The sequence of <typeparamref name="T" /></param>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="paginationFilter">Pagination filter to apply if any. Note that this will NOT override an already set filter.</param>
        /// <returns>A filtered sequence of <typeparamref name="TResult" /></returns>
        IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> inputQuery, ISpecification<T, TResult> specification, PaginationFilter? paginationFilter = null)
            where T : class where TResult : class;

        /// <summary>
        ///     Applies the logic encapsulated by <paramref name="specification" /> to given <paramref name="inputQuery" />.
        /// </summary>
        /// <param name="inputQuery">The sequence of <typeparamref name="T" /></param>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="paginationFilter">Pagination filter to apply if any. Note that this will NOT override an already set filter.</param>
        /// <param name="evaluateCriteriaOnly">Whether to only evaluate criteria.</param>
        /// <returns>A filtered sequence of <typeparamref name="T" /></returns>
        IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification, PaginationFilter? paginationFilter = null,
            bool evaluateCriteriaOnly = false) where T : class;
    }
}