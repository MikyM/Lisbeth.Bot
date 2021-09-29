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
        protected DbContext _context;
        protected readonly DbSet<TEntity> _set;

        protected ReadOnlyRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _set = _context.Set<TEntity>();
        }

        public virtual async ValueTask<TEntity> GetAsync(params object[] keyValues)
        {
            return await _set.FindAsync(keyValues);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecificationsAsync(
            ISpecifications<TEntity> baseSpecifications = null)
        {
            return await SpecificationEvaluator<TEntity>
                .GetQuery(_set.AsQueryable(), baseSpecifications)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetBySpecificationsAsync(PaginationFilter filter,
            ISpecifications<TEntity> baseSpecifications = null)
        {
            return await SpecificationEvaluator<TEntity>
                .GetQuery(_set.AsQueryable(), baseSpecifications)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<long> LongCountAsync(ISpecifications<TEntity> specifications = null)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_set.AsQueryable(), specifications)
                .AsNoTracking()
                .LongCountAsync();
        }
    }
}
