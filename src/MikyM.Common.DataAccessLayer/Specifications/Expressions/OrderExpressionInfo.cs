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

using MikyM.Common.DataAccessLayer.Specifications.Helpers;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Expressions
{
    /// <summary>
    /// Encapsulates data needed to perform sorting.
    /// </summary>
    /// <typeparam name="T">Type of the entity to apply sort on.</typeparam>
    public class OrderExpressionInfo<T>
    {
        private readonly Lazy<Func<T, object?>> _keySelectorFunc;

        /// <summary>
        /// Creates instance of <see cref="OrderExpressionInfo{T}" />.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="orderType">Whether to (subsequently) sort ascending or descending.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="keySelector"/> is null.</exception>
        public OrderExpressionInfo(Expression<Func<T, object?>> keySelector, OrderTypeEnum orderType)
        {
            _ = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

            this.KeySelector = keySelector;
            this.OrderType = orderType;

            this._keySelectorFunc = new Lazy<Func<T, object?>>(this.KeySelector.Compile);
        }

        /// <summary>
        /// A function to extract a key from an element.
        /// </summary>
        public Expression<Func<T, object?>> KeySelector { get; }

        /// <summary>
        /// Whether to (subsequently) sort ascending or descending.
        /// </summary>
        public OrderTypeEnum OrderType { get; }

        /// <summary>
        /// Compiled <see cref="KeySelector" />.
        /// </summary>
        public Func<T, object?> KeySelectorFunc => this._keySelectorFunc.Value;
    }
}
