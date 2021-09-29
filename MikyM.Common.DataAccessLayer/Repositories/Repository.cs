using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer.Helpers;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.Repositories
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity> where TEntity : AggregateRootEntity
    {
        protected Repository(DbContext context) : base(context)
        {
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _set.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _set.AddRangeAsync(entities);
        }

        public virtual void AddOrUpdate(TEntity entity)
        {
            _set.Update(entity);
        }

        public virtual void AddOrUpdateRange(IEnumerable<TEntity> entities)
        {
            _set.UpdateRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Attach(entity).State = EntityState.Modified;
        }


        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _context.Attach(entity).State = EntityState.Modified;
            }
        }

        public virtual void Delete(TEntity entity)
        {
            _set.Remove(entity);
        }

        public virtual void Delete(long id)
        {
            var entity = _context.FindTracked<TEntity>(id) ?? (TEntity) Activator.CreateInstance(typeof(TEntity), id);
            _set.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            _set.RemoveRange(entities);
        }

        public virtual void DeleteRange(IEnumerable<long> ids)
        {
            var entities = ids.Select(id => _context.FindTracked<TEntity>(id) ?? (TEntity) Activator.CreateInstance(typeof(TEntity), id)).ToList();
            _set.RemoveRange(entities);
        }

        public virtual void Disable(TEntity entity)
        {
            entity.IsDisabled = true;
            Update(entity);
        }

        public virtual async Task DisableAsync(long id)
        {
            var entity = await GetAsync(id);
            entity.IsDisabled = true;
            Update(entity);
        }

        public virtual void DisableRange(IEnumerable<TEntity> entities)
        {
            var aggregateRootEntities = entities.ToList();
            foreach (var entity in aggregateRootEntities)
            {
                entity.IsDisabled = true;
            }
            UpdateRange(aggregateRootEntities);
        }

        public virtual async Task DisableRangeAsync(IEnumerable<long> ids)
        {
            var entities = await _set.Join(ids, ent => ent.Id, id => id, (ent, id) => ent).ToListAsync();
            entities.ForEach(ent => ent.IsDisabled = true);
            UpdateRange(entities);
        }
    }
}
