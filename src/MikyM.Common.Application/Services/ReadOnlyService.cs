// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.Application.Services
{
    public class ReadOnlyService<TEntity, TContext> : ServiceBase<TContext>, IReadOnlyService<TEntity, TContext>
        where TEntity : AggregateRootEntity where TContext : DbContext
    {
        public ReadOnlyService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
        {
        }

        public virtual async Task<TGetResult> GetAsync<TGetResult>(long id) where TGetResult : class
        {
            return _mapper.Map<TGetResult>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>().GetAsync(id));
        }

        public virtual async Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(
            ISpecifications<TEntity> specifications = null) where TGetResult : class
        {
            return _mapper.Map<IReadOnlyList<TGetResult>>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                .GetBySpecificationsAsync(specifications));
        }

        public virtual async Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(
            PaginationFilterDto filter, ISpecifications<TEntity> specifications = null) where TGetResult : class
        {
            return _mapper.Map<IReadOnlyList<TGetResult>>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                .GetBySpecificationsAsync(_mapper.Map<PaginationFilter>(filter), specifications));
        }

        public virtual async Task<long> LongCountAsync(ISpecifications<TEntity> specifications = null)
        {
            return await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>().LongCountAsync(specifications);
        }
    }
}