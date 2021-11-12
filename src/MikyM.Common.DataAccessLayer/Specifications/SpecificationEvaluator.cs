﻿// This file is part of Lisbeth.Bot project
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

namespace MikyM.Common.DataAccessLayer.Specifications;

/// <inheritdoc cref="ISpecificationEvaluator" />
public class SpecificationEvaluator : ISpecificationEvaluator
{
    private readonly List<IEvaluator> _evaluators = new();

    public SpecificationEvaluator()
    {
        _evaluators.AddRange(new IEvaluator[]
        {
            WhereEvaluator.Instance, SearchEvaluator.Instance, IncludeEvaluator.Instance,
            OrderEvaluator.Instance, PaginationEvaluator.Instance, AsNoTrackingEvaluator.Instance,
            AsSplitQueryEvaluator.Instance, AsNoTrackingWithIdentityResolutionEvaluator.Instance,
            GroupByEvaluator.Instance, CachingEvaluator.Instance
        });
    }

    public SpecificationEvaluator(IEnumerable<IEvaluator> evaluators)
    {
        _evaluators.AddRange(evaluators);
    }

    // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
    public static SpecificationEvaluator Default { get; } = new();

    public virtual IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query,
        ISpecification<T, TResult> specification) where T : class where TResult : class
    {
        query = GetQuery(query, (ISpecification<T>)specification);

        return ProjectionEvaluator.Instance.GetQuery(query, specification);
    }

    public virtual IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification,
        bool evaluateCriteriaOnly = false) where T : class
    {
        return (evaluateCriteriaOnly ? _evaluators.Where(x => x.IsCriteriaEvaluator) : _evaluators).Aggregate(query,
            (current, evaluator) => evaluator.GetQuery(current, specification));
    }
}