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

using System.Collections.Generic;
using System.Threading.Tasks;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.Repositories
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : AggregateRootEntity
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void AddOrUpdate(TEntity entity);
        void AddOrUpdateRange(IEnumerable<TEntity> entities);
        void BeginUpdate(TEntity entity);
        void BeginUpdateRange(IEnumerable<TEntity> entities);
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
