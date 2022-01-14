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

using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Expressions;

/// <summary>
/// Encapsulates data needed to perform filtering.
/// </summary>
/// <typeparam name="T">Type of the entity to apply filter on.</typeparam>
public class WhereExpressionInfo<T>
{
    private readonly Lazy<Func<T, bool>> _filterFunc;

    /// <summary>
    /// Creates instance of <see cref="WhereExpressionInfo{T}" />.
    /// </summary>
    /// <param name="filter">Condition which should be satisfied by instances of <typeparamref name="T"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="filter"/> is null.</exception>
    public WhereExpressionInfo(Expression<Func<T, bool>> filter)
    {
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        this.Filter = filter;

        this._filterFunc = new Lazy<Func<T, bool>>(this.Filter.Compile);
    }

    /// <summary>
    /// Condition which should be satisfied by instances of <typeparamref name="T"/>.
    /// </summary>
    public Expression<Func<T, bool>> Filter { get; }

    /// <summary>
    /// Compiled <see cref="Filter" />.
    /// </summary>
    public Func<T, bool> FilterFunc => this._filterFunc.Value;
}