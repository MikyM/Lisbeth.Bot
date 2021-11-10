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

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikyM.Common.Application.Services
{
    public class ReadOnlyService<TEntity, TContext> : ServiceBase<TContext>, IReadOnlyService<TEntity, TContext>
        where TEntity : AggregateRootEntity where TContext : DbContext
    {
        public ReadOnlyService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
        {
        }

        public virtual async Task<Result<TGetResult>> GetAsync<TGetResult>(long id) where TGetResult : class
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()!.GetAsync(id);
            return res is null ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(Mapper.Map<TGetResult>(res));
        }

        public virtual async Task<Result<TGetResult>> GetSingleBySpecAsync<TGetResult>(ISpecification<TEntity> specification) where TGetResult : class
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                ?.GetSingleBySpecAsync(specification)!;
            return res is null ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(Mapper.Map<TGetResult>(res));
        }

        public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetBySpecAsync<TGetResult>(ISpecification<TEntity> specification) where TGetResult : class
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                ?.GetBySpecAsync(specification)!;
            return res.Count is 0 ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetResult>>.FromSuccess(Mapper.Map<IReadOnlyList<TGetResult>>(res));
        }

        public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetBySpecAsync<TGetResult>(
            PaginationFilterDto filter, ISpecification<TEntity> specification) where TGetResult : class
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                ?.GetBySpecAsync(Mapper.Map<PaginationFilter>(filter), specification)!;
            return res.Count is 0 ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetResult>>.FromSuccess(Mapper.Map<IReadOnlyList<TGetResult>>(res));
        }

        public async Task<Result<IReadOnlyList<TGetResult>>> GetAnyAsync<TGetResult>(PaginationFilterDto? filter = null) where TGetResult : class
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()?.GetAnyAsync(filter is null ? null : Mapper.Map<PaginationFilter>(filter))!;
            return res.Count is 0 ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetResult>>.FromSuccess(Mapper.Map<IReadOnlyList<TGetResult>>(res));
        }

        public virtual async Task<Result<long>> LongCountAsync(ISpecification<TEntity>? specification = null)
        {
            var res = await UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()?.LongCountAsync(specification)!;
            return Result<long>.FromSuccess(res);
        }
    }
}