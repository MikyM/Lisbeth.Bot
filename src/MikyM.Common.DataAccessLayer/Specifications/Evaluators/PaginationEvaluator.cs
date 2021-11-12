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

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators;

public class PaginationEvaluator : IEvaluator, IInMemoryEvaluator
{
    private PaginationEvaluator()
    {
    }

    public static PaginationEvaluator Instance { get; } = new();

    public bool IsCriteriaEvaluator { get; } = false;

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (!specification.IsPagingEnabled) return query;

        // If skip is 0, avoid adding to the IQueryable. It will generate more optimized SQL that way.
        if (specification.Skip is not null && specification.Skip != 0) query = query.Skip(specification.Skip.Value);

        if (specification.Take is not null) query = query.Take(specification.Take.Value);

        return query;
    }

    public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification) where T : class
    {
        if (!specification.IsPagingEnabled) return query;

        if (specification.Skip is not null && specification.Skip != 0) query = query.Skip(specification.Skip.Value);

        if (specification.Take is not null) query = query.Take(specification.Take.Value);

        return query;
    }
}