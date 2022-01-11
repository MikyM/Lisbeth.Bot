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

namespace MikyM.Common.Utilities.ExpressionHelpers;

public static class ExpressionExtensions
{
    /// <summary>
    /// Convert a lambda expression for a getter into a setter
    /// </summary>
    public static Action<TEntity, TValue> GetSetter<TEntity, TValue>(this Expression<Func<TEntity, TValue>> expression)
    {
        var parameter = Expression.Parameter(typeof(TValue), "value");
        var setterLambda = Expression.Lambda<Action<TEntity, TValue>>(Expression.Assign(expression.Body, parameter),
            expression.Parameters[0], parameter);

        return setterLambda.Compile();
    }
}
