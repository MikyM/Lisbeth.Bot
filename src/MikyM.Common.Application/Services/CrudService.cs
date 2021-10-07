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

            TEntity entity;

            if (entry is TEntity rootEntity)
            {
                entity = rootEntity;
                _unitOfWork.GetRepository<Repository<TEntity>>().Add(entity);
            }
            else
            {
                entity = _mapper.Map<TEntity>(entry);
                _unitOfWork.GetRepository<Repository<TEntity>>().Add(entity);
            }
            
            if (!shouldSave) return 0;
            await CommitAsync();
            return entity.Id;
        }

        public virtual async Task<IEnumerable<long>> AddRangeAsync<TPost>(IEnumerable<TPost> entries, bool shouldSave = false) where TPost : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            IEnumerable<TEntity> entities;

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                entities = rootEntities;
                _unitOfWork.GetRepository<Repository<TEntity>>().AddRange(entities);
            }
            else
            {
                entities = _mapper.Map<IEnumerable<TEntity>>(entries);
                _unitOfWork.GetRepository<Repository<TEntity>>().AddRange(entities);
            }
            
            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public virtual void BeginUpdate<TPatch>(TPatch entry) where TPatch : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            if (entry is TEntity rootEntity)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().BeginUpdate(rootEntity);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().BeginUpdate(_mapper.Map<TEntity>(entry));
            }
        }

        public virtual void BeginUpdateRange<TPatch>(IEnumerable<TPatch> entries) where TPatch : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().BeginUpdateRange(rootEntities);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().BeginUpdateRange(_mapper.Map<IEnumerable<TEntity>>(entries));
            }
        }

        public virtual async Task<long> AddOrUpdateAsync<TPut>(TPut entry, bool shouldSave = false) where TPut : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            TEntity entity;

            if (entry is TEntity rootEntity)
            {
                entity = rootEntity;
                _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdate(entity);
            }
            else
            {
                entity = _mapper.Map<TEntity>(entry);
                _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdate(entity);
            }

            if (!shouldSave) return 0;
            await CommitAsync();
            return entity.Id;
        }

        public virtual async Task<List<long>> AddOrUpdateRangeAsync<TPut>(IEnumerable<TPut> entries, bool shouldSave = false) where TPut : class
        {
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            IEnumerable<TEntity> entities;

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                entities = rootEntities;
                _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdateRange(entities);
            }
            else
            {
                entities = _mapper.Map<IEnumerable<TEntity>>(entries);
                _unitOfWork.GetRepository<Repository<TEntity>>().AddOrUpdateRange(_mapper.Map<IEnumerable<TEntity>>(entities));
            }

            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public virtual async Task DeleteAsync<TDelete>(TDelete entry, bool shouldSave = false) where TDelete : class
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            if (entry is TEntity rootEntity)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().Delete(rootEntity);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().Delete(_mapper.Map<TEntity>(entry));
            }

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

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().DeleteRange(rootEntities);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().DeleteRange(_mapper.Map<IEnumerable<TEntity>>(entries));
            }

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

            if (entry is TEntity rootEntity)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().Disable(rootEntity);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().Disable(_mapper.Map<TEntity>(entry));
            }

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

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().DisableRange(rootEntities);
            }
            else
            {
                _unitOfWork.GetRepository<Repository<TEntity>>().DeleteRange(_mapper.Map<IEnumerable<TEntity>>(entries));
            }

            if (shouldSave) await CommitAsync();
        }
    }
}
