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
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications.Builders;

public static class OrderedBuilderExtensions
{
    public static IOrderedSpecificationBuilder<T> ThenBy<T>(
        this IOrderedSpecificationBuilder<T> orderedBuilder,
        Expression<Func<T, object?>> orderExpression) where T : class
    {
        orderedBuilder.Specification.OrderExpressions ??=
            new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();
        ((List<(Expression<Func<T, object?>> OrderExpression, OrderTypeEnum OrderType)>) orderedBuilder
                .Specification.OrderExpressions)
            .Add((orderExpression, OrderTypeEnum.ThenBy));

        return orderedBuilder;
    }

    public static IOrderedSpecificationBuilder<T> ThenByDescending<T>(
        this IOrderedSpecificationBuilder<T> orderedBuilder,
        Expression<Func<T, object?>> orderExpression) where T : class
    {
        orderedBuilder.Specification.OrderExpressions ??=
            new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();
        ((List<(Expression<Func<T, object?>> OrderExpression, OrderTypeEnum OrderType)>) orderedBuilder
                .Specification.OrderExpressions)
            .Add((orderExpression, OrderTypeEnum.ThenByDescending));

        return orderedBuilder;
    }
}