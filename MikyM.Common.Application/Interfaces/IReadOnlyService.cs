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