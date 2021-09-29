using System.Collections.Generic;
using System.Threading.Tasks;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.Repositories
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : AggregateRootEntity
    {
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void AddOrUpdate(TEntity entity);
        void AddOrUpdateRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void Delete(long id);
        void DeleteRange(IEnumerable<TEntity> entities);
        void DeleteRange(IEnumerable<long> ids);
        void Disable(TEntity entity);
        Task DisableAsync(long id);
        void DisableRange(IEnumerable<TEntity> entities);
        Task DisableRangeAsync(IEnumerable<long> ids);
    }
}
