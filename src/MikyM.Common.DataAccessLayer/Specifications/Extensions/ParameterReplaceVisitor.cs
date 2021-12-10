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

namespace MikyM.Common.DataAccessLayer.Specifications.Extensions;

internal class ParameterReplacerVisitor : ExpressionVisitor
{
    private readonly Expression newExpression;
    private readonly ParameterExpression oldParameter;

    private ParameterReplacerVisitor(ParameterExpression oldParameter, Expression newExpression)
    {
        this.oldParameter = oldParameter;
        this.newExpression = newExpression;
    }

    internal static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
    {
        return new ParameterReplacerVisitor(oldParameter, newExpression).Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        return p == oldParameter ? newExpression : p;
    }
}
