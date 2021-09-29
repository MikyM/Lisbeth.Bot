using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MikyM.Common.Application.Services
{
    public class CrudService<TEntity, TContext> : ReadOnlyService<TEntity, TContext>, ICrudService<TEntity, TContext> where TEntity : AggregateRootEntity where TContext : DbContext
    {
        public CrudService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
        {
        }

        public virtual async Task<long> AddAsync<TPost>(TPost entry, bool shouldSave = false) where TPost : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            var entity = _mapper.Map<TEntity>(entry);
            await _unitOfWork.GetRepository<Repository<TEntity>>().AddAsync(entity);

            if (!shouldSave) return 0;
            await CommitAsync();
            return entity.Id;
        }

        public virtual async Task<IEnumerable<long>> AddRangeAsync<TPost>(IEnumerable<TPost> entries, bool shouldSave = false) where TPost : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            var entities = _mapper.Map<IEnumerable<TEntity>>(entries).ToList();
            await _unitOfWork.GetRepository<Repository<TEntity>>().AddRangeAsync(entities);

            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public virtual async Task UpdateAsync<TPatch>(TPatch entry, bool shouldSave = false) where TPatch : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            _unitOfWork.GetRepository<Repository<TEntity>>().Update(_mapper.Map<TEntity>(entry));

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task UpdateRangeAsync<TPatch>(IEnumerable<TPatch> entries, bool shouldSave = false) where TPatch : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            _unitOfWork.GetRepository<Repository<TEntity>>().UpdateRange(_mapper.Map<IEnumerable<TEntity>>(entries));

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task<long> AddOrUpdateAsync<TPut>(TPut entry, bool shouldSave = false) where TPut : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            var entity = _mapper.Map<TEntity>(entry);
            _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdate(entity);

            if (!shouldSave) return 0;
            await CommitAsync();
            return entity.Id;
        }

        public virtual async Task<List<long>> AddOrUpdateRangeAsync<TPut>(IEnumerable<TPut> entries, bool shouldSave = false) where TPut : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            var entities = _mapper.Map<IEnumerable<TEntity>>(entries).ToList();
            _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdateRange(entities);

            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public virtual async Task DeleteAsync<TDelete>(TDelete entry, bool shouldSave = false) where TDelete : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            _unitOfWork.GetRepository<Repository<TEntity>>().Delete(_mapper.Map<TEntity>(entry));

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DeleteAsync(long id, bool shouldSave = false)
        {
            _unitOfWork.GetRepository<Repository<TEntity>>().Delete(id);

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<long> ids, bool shouldSave = false)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            _unitOfWork.GetRepository<Repository<TEntity>>().DeleteRange(ids);

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DeleteRangeAsync<TDelete>(IEnumerable<TDelete> entries, bool shouldSave = false) where TDelete : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            _unitOfWork.GetRepository<Repository<TEntity>>()
                .DeleteRange(_mapper.Map<IEnumerable<TEntity>>(entries));

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DisableAsync(long id, bool shouldSave = false)
        {
            await _unitOfWork.GetRepository<Repository<TEntity>>()
                .DisableAsync(id);

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DisableAsync<TDisable>(TDisable entry, bool shouldSave = false) where TDisable : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            _unitOfWork.GetRepository<Repository<TEntity>>()
                .Disable(_mapper.Map<TEntity>(entry));

            if (shouldSave) await CommitAsync();
        }
        public virtual async Task DisableRangeAsync(IEnumerable<long> ids, bool shouldSave = false)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            await _unitOfWork.GetRepository<Repository<TEntity>>()
                .DisableRangeAsync(ids);

            if (shouldSave) await CommitAsync();
        }

        public virtual async Task DisableRangeAsync<TDisable>(IEnumerable<TDisable> entries, bool shouldSave = false) where TDisable : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            var aggregateRootEntities = _mapper.Map<IEnumerable<TEntity>>(entries);
            _unitOfWork.GetRepository<Repository<TEntity>>()
                .DisableRange(aggregateRootEntities);

            if (shouldSave) await CommitAsync();
        }
    }
}
