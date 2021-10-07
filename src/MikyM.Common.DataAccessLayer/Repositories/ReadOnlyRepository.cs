// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Specifications;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.Repositories
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : AggregateRootEntity
    {
        public readonly DbContext _context;

        public ReadOnlyRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async ValueTask<TEntity> GetAsync(params object[] keyValues)
        {
            return await _context.Set<TEntity>().FindAsync(keyValues);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecificationsAsync(
            ISpecifications<TEntity> baseSpecifications = null)
        {
            return await SpecificationEvaluator<TEntity>
                .GetQuery(_context.Set<TEntity>().AsQueryable(), baseSpecifications)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecificationsAsync(PaginationFilter filter,
            ISpecifications<TEntity> baseSpecifications = null)
        {
            return await SpecificationEvaluator<TEntity>
                .GetQuery(_context.Set<TEntity>().AsQueryable(), baseSpecifications)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<long> LongCountAsync(ISpecifications<TEntity> specifications = null)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specifications)
                .AsNoTracking()
                .LongCountAsync();
        }
    }
}
