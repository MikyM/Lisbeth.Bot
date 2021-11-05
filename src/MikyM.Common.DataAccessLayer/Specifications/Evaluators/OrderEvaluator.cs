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
using System.Linq;
using System.Linq.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Exceptions;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators
{
    public class OrderEvaluator : IEvaluator, IInMemoryEvaluator
    {
        private OrderEvaluator()
        {
        }

        public static OrderEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator { get; } = false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            if (specification.OrderExpressions is null) return query;
            if (specification.OrderExpressions.Count(x =>
                x.OrderType is OrderTypeEnum.OrderBy or OrderTypeEnum.OrderByDescending) > 1)
                throw new DuplicateOrderChainException();

            IOrderedQueryable<T>? orderedQuery =
                specification.OrderExpressions
                    .Aggregate<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType), IOrderedQueryable<T>
                        ?>(null, (current, orderExpression) => orderExpression.OrderType switch
                    {
                        OrderTypeEnum.OrderBy => query.OrderBy(orderExpression.KeySelector),
                        OrderTypeEnum.OrderByDescending => query.OrderByDescending(orderExpression.KeySelector),
                        OrderTypeEnum.ThenBy => current.ThenBy(orderExpression.KeySelector),
                        OrderTypeEnum.ThenByDescending => current.ThenByDescending(orderExpression.KeySelector),
                        _ => current
                    });

            if (orderedQuery is not null) query = orderedQuery;

            return query;
        }

        public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification) where T : class
        {
            if (specification.OrderExpressions is null) return query;
            if (specification.OrderExpressions.Count(x =>
                x.OrderType is OrderTypeEnum.OrderBy or OrderTypeEnum.OrderByDescending) > 1)
                throw new DuplicateOrderChainException();

            IOrderedEnumerable<T>? orderedQuery =
                specification.OrderExpressions
                    .Aggregate<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType), IOrderedEnumerable<T>
                        ?>(null, (current, orderExpression) => orderExpression.OrderType switch
                    {
                        OrderTypeEnum.OrderBy => query.OrderBy(orderExpression.KeySelector.Compile()),
                        OrderTypeEnum.OrderByDescending => query.OrderByDescending(
                            orderExpression.KeySelector.Compile()),
                        OrderTypeEnum.ThenBy => current.ThenBy(orderExpression.KeySelector.Compile()),
                        OrderTypeEnum.ThenByDescending => current.ThenByDescending(
                            orderExpression.KeySelector.Compile()),
                        _ => current
                    });

            if (orderedQuery is not null) query = orderedQuery;

            return query;
        }
    }
}