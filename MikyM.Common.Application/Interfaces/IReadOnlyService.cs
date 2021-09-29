using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MikyM.Common.Application.Interfaces
{
    public interface IReadOnlyService<TEntity, TContext> : IServiceBase<TContext> where TEntity : AggregateRootEntity where TContext : DbContext
    {
        Task<TGetResult> GetAsync<TGetResult>(long id) where TGetResult : class;

        Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(
            ISpecifications<TEntity> specifications = null) where TGetResult : class;

        Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(PaginationFilterDto filter,
            ISpecifications<TEntity> specifications = null) where TGetResult : class;

        Task<long> LongCountAsync(ISpecifications<TEntity> specifications = null);
    }
}