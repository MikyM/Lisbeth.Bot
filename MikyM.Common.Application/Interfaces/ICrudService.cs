using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.Application.Interfaces
{
    public interface ICrudService<TEntity, TContext> : IReadOnlyService<TEntity, TContext> where TEntity : AggregateRootEntity where TContext : DbContext
    {
        Task<long> AddAsync<TPost>(TPost entry, bool shouldSave = false) where TPost : class;
        Task<IEnumerable<long>> AddRangeAsync<TPost>(IEnumerable<TPost> entries, bool shouldSave = false) where TPost : class;

        Task UpdateAsync<TPatch>(TPatch entry, bool shouldSave = false) where TPatch : class;

        Task UpdateRangeAsync<TPatch>(IEnumerable<TPatch> entries, bool shouldSave = false)
            where TPatch : class;

        Task<long> AddOrUpdateAsync<TPut>(TPut entry, bool shouldSave = false) where TPut : class;

        Task<List<long>> AddOrUpdateRangeAsync<TPut>(IEnumerable<TPut> entries, bool shouldSave = false)
            where TPut : class;

        Task DeleteAsync<TDelete>(TDelete entry, bool shouldSave = false) where TDelete : class;
        Task DeleteAsync(long id, bool shouldSave = false);

        Task DeleteRangeAsync<TDelete>(IEnumerable<TDelete> entries, bool shouldSave = false)
            where TDelete : class;

        Task DeleteRangeAsync(IEnumerable<long> ids, bool shouldSave = false);
        Task DisableAsync<TDisable>(TDisable entry, bool shouldSave = false) where TDisable : class;
        Task DisableAsync(long id, bool shouldSave = false);

        Task DisableRangeAsync<TDisable>(IEnumerable<TDisable> entries, bool shouldSave = false)
            where TDisable : class;

        Task DisableRangeAsync(IEnumerable<long> ids, bool shouldSave = false);
    }
}
