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
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    /// <inheritdoc cref="ISpecificationEvaluator"/>
    public class SpecificationEvaluator : ISpecificationEvaluator
    {
        // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
        public static SpecificationEvaluator Default { get; } = new ();

        private readonly List<IEvaluator> _evaluators = new ();

        public SpecificationEvaluator()
        {
            this._evaluators.AddRange(new IEvaluator[]
            {
                WhereEvaluator.Instance, SearchEvaluator.Instance, IncludeEvaluator.Instance,
                OrderEvaluator.Instance, PaginationEvaluator.Instance, AsNoTrackingEvaluator.Instance,
                AsSplitQueryEvaluator.Instance, AsNoTrackingWithIdentityResolutionEvaluator.Instance, GroupByEvaluator.Instance
            });
        }

        public SpecificationEvaluator(IEnumerable<IEvaluator> evaluators)
        {
            this._evaluators.AddRange(evaluators);
        }

        public virtual IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query,
            ISpecification<T, TResult> specification) where T : class
        {
            query = GetQuery(query, (ISpecification<T>) specification);

            return query.Select(specification.Selector ?? throw new InvalidOperationException());
        }

        public virtual IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification,
            bool evaluateCriteriaOnly = false) where T : class
        {
            return (evaluateCriteriaOnly ? this._evaluators.Where(x => x.IsCriteriaEvaluator) : this._evaluators)
                .Aggregate(query, (current, evaluator) => evaluator.GetQuery(current, specification));
        }
    }
}
