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
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.Application.Interfaces
{
    public interface ICrudService<TEntity, TContext> : IReadOnlyService<TEntity, TContext> where TEntity : AggregateRootEntity where TContext : DbContext
    {
        Task<long> AddAsync<TPost>(TPost entry, bool shouldSave = false) where TPost : class;
        Task<IEnumerable<long>> AddRangeAsync<TPost>(IEnumerable<TPost> entries, bool shouldSave = false) where TPost : class;

        void BeginUpdate<TPatch>(TPatch entry) where TPatch : class;

        void BeginUpdateRange<TPatch>(IEnumerable<TPatch> entries)
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
