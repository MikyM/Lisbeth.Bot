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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;
using MikyM.Common.DataAccessLayer.Specifications.Exceptions;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.Repositories
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : AggregateRootEntity
    {
        public readonly DbContext _context;
        private readonly ISpecificationEvaluator _specificationEvaluator;

        public ReadOnlyRepository(DbContext context, ISpecificationEvaluator specificationEvaluator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _specificationEvaluator = specificationEvaluator;
        }

        public virtual async ValueTask<TEntity> GetAsync(params object[] keyValues)
        {
            return await _context.Set<TEntity>().FindAsync(keyValues);
        }
        
        public virtual async Task<TEntity> GetSingleBySpecAsync<TSpec>(
            ISpecification<TEntity> specification = null) where TSpec : ISpecification<TEntity>, ISingleResultSpecification
        {
            return await ApplySpecification(specification)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<TProjectTo> GetSingleBySpecAsync<TSpec, TProjectTo>(
            ISpecification<TEntity, TProjectTo> specification = null) where TSpec : ISpecification<TEntity, TProjectTo>, ISingleResultSpecification where TProjectTo : class
        {
            return await ApplySpecification(specification)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecAsync(ISpecification<TEntity> specification = null)
        {
            var result = await ApplySpecification(specification).ToListAsync();
            return specification?.PostProcessingAction is null
                ? result
                : specification.PostProcessingAction(result).ToList();
        }

        public virtual async Task<IReadOnlyList<TProjectTo>> GetBySpecAsync<TProjectTo>(ISpecification<TEntity, TProjectTo> specification = null) where TProjectTo : class
        {
            var result = await ApplySpecification(specification).ToListAsync();
            return specification?.PostProcessingAction is null
                ? result
                : specification.PostProcessingAction(result).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecAsync(PaginationFilter filter,
            ISpecification<TEntity> specification = null)
        {
            var result = await ApplySpecification(specification)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
            return specification?.PostProcessingAction is null
                ? result
                : specification.PostProcessingAction(result).ToList();
        }

        public virtual async Task<IReadOnlyList<TProjectTo>> GetBySpecAsync<TProjectTo>(PaginationFilter filter,
            ISpecification<TEntity, TProjectTo> specification = null) where TProjectTo : class
        {
            var result = await ApplySpecification(specification)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
            return specification?.PostProcessingAction is null
                ? result
                : specification.PostProcessingAction(result).ToList();
        }

        public virtual async Task<long> LongCountAsync(ISpecification<TEntity> specification = null)
        {
            return await ApplySpecification(specification)
                .LongCountAsync();
        }

        /// <summary>
        /// Filters the entities  of <typeparamref name="TEntity"/>, to those that match the encapsulated query logic of the
        /// <paramref name="specification"/>.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>The filtered entities as an <see cref="IQueryable{T}"/>.</returns>
        protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool evaluateCriteriaOnly = false)
        {
            return _specificationEvaluator.GetQuery(_context.Set<TEntity>().AsQueryable(), specification, evaluateCriteriaOnly);
        }
        /// <summary>
        /// Filters all entities of <typeparamref name="TEntity" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// <para>
        /// Projects each entity into a new form, being <typeparamref name="TResult" />.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the projection.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>The filtered projected entities as an <see cref="IQueryable{T}"/>.</returns>
        protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification)
        {
            if (specification  is null) throw new ArgumentNullException("Specification is required");
            if (specification.Selector  is null) throw new SelectorNotFoundException();

            return _specificationEvaluator.GetQuery(_context.Set<TEntity>().AsQueryable(), specification);
        }
    }
}