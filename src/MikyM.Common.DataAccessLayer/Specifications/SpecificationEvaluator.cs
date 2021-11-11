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

using AutoMapper.QueryableExtensions;
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;
using System.Collections.Generic;
using System.Linq;
using EFCoreSecondLevelCacheInterceptor;
using MikyM.Common.DataAccessLayer.Filters;

namespace MikyM.Common.DataAccessLayer.Specifications
{
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
                GroupByEvaluator.Instance
            });
        }

        public SpecificationEvaluator(IEnumerable<IEvaluator> evaluators)
        {
            _evaluators.AddRange(evaluators);
        }

        // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
        public static SpecificationEvaluator Default { get; } = new();

        public virtual IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query,
            ISpecification<T, TResult> specification, PaginationFilter? paginationFilter = null) where T : class where TResult : class
        {
            query = GetQuery(query, (ISpecification<T>)specification, paginationFilter);

            if (specification.MembersToExpand is not null)
            {
                return specification.MapperConfiguration is null
                    ? query.ProjectTo<TResult>(specification.MembersToExpand.ToArray())
                    : query.ProjectTo<TResult>(specification.MapperConfiguration,
                        specification.MembersToExpand.ToArray());
            }

            if (specification.StringMembersToExpand is not null)
            {
                return specification.MapperConfiguration is null
                    ? query.ProjectTo<TResult>(null, specification.StringMembersToExpand.ToArray())
                    : query.ProjectTo<TResult>(specification.MapperConfiguration, null,
                        specification.StringMembersToExpand.ToArray());
            }

            return specification.MapperConfiguration is not null
                ? query.ProjectTo<TResult>(specification.MapperConfiguration)
                : query.ProjectTo<TResult>();
        }

        public virtual IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification,
            PaginationFilter? paginationFilter = null, bool evaluateCriteriaOnly = false) where T : class
        {
            if (specification.IsCacheEnabled.HasValue) query = !specification.IsCacheEnabled.Value ? query.NotCacheable() : query.Cacheable();

            if (paginationFilter is not null && specification.PaginationFilter is null) ((Specification<T>)specification).PaginationFilter = paginationFilter;
            
            return (evaluateCriteriaOnly ? _evaluators.Where(x => x.IsCriteriaEvaluator) : _evaluators)
                .Aggregate(query, (current, evaluator) => evaluator.GetQuery(current, specification));
        }
    }
}