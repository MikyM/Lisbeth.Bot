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

using MikyM.Common.DataAccessLayer.Specifications.Exceptions;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;
using System.Collections.Generic;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators;

public class OrderEvaluator : IEvaluator, IInMemoryEvaluator
{
    private OrderEvaluator() { }
    public static OrderEvaluator Instance { get; } = new OrderEvaluator();

    public bool IsCriteriaEvaluator { get; } = false;

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.OrderExpressions != null)
        {
            if (specification.OrderExpressions.Count(x => x.OrderType == OrderTypeEnum.OrderBy
                    || x.OrderType == OrderTypeEnum.OrderByDescending) > 1)
            {
                throw new DuplicateOrderChainException();
            }

            IOrderedQueryable<T>? orderedQuery = null;
            foreach (var orderExpression in specification.OrderExpressions)
            {
                if (orderExpression.OrderType == OrderTypeEnum.OrderBy)
                {
                    orderedQuery = query.OrderBy(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.OrderByDescending)
                {
                    orderedQuery = query.OrderByDescending(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenBy)
                {
                    orderedQuery = orderedQuery.ThenBy(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenByDescending)
                {
                    orderedQuery = orderedQuery.ThenByDescending(orderExpression.KeySelector);
                }
            }

            if (orderedQuery != null)
            {
                query = orderedQuery;
            }
        }

        return query;
    }

    public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.OrderExpressions != null)
        {
            if (specification.OrderExpressions.Count(x => x.OrderType == OrderTypeEnum.OrderBy
                    || x.OrderType == OrderTypeEnum.OrderByDescending) > 1)
            {
                throw new DuplicateOrderChainException();
            }

            IOrderedEnumerable<T>? orderedQuery = null;
            foreach (var orderExpression in specification.OrderExpressions)
            {
                if (orderExpression.OrderType == OrderTypeEnum.OrderBy)
                {
                    orderedQuery = query.OrderBy(orderExpression.KeySelectorFunc);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.OrderByDescending)
                {
                    orderedQuery = query.OrderByDescending(orderExpression.KeySelectorFunc);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenBy)
                {
                    orderedQuery = orderedQuery.ThenBy(orderExpression.KeySelectorFunc);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenByDescending)
                {
                    orderedQuery = orderedQuery.ThenByDescending(orderExpression.KeySelectorFunc);
                }
            }

            if (orderedQuery != null)
            {
                query = orderedQuery;
            }
        }

        return query;
    }
}