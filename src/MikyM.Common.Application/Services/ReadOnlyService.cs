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
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Common.DataAccessLayer.Specifications;

namespace MikyM.Common.Application.Services;

public class ReadOnlyService<TEntity, TContext> : ServiceBase<TContext>, IReadOnlyService<TEntity, TContext>
    where TEntity : AggregateRootEntity where TContext : DbContext
{
    public ReadOnlyService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
    {
    }

    public virtual async Task<Result<TGetResult>> GetAsync<TGetResult>(long id) where TGetResult : class
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()!.GetAsync(id);
        return res is null ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(this.Mapper.Map<TGetResult>(res));
    }

    public virtual async Task<Result<TGetResult>> GetSingleBySpecAsync<TGetResult>(ISpecification<TEntity> specification) where TGetResult : class
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetSingleBySpecAsync(specification)!;
        return res is null ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(this.Mapper.Map<TGetResult>(res));
    }

    public virtual async Task<Result<TGetProjectedResult>> GetSingleBySpecAsync<TGetProjectedResult>(ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetSingleBySpecAsync(specification)!;
        return res is null ? Result<TGetProjectedResult>.FromError(new NotFoundError()) : Result<TGetProjectedResult>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetBySpecAsync<TGetResult>(ISpecification<TEntity> specification) where TGetResult : class
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetBySpecAsync(specification)!;
        return res.Count is 0 ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetResult>>.FromSuccess(this.Mapper.Map<IReadOnlyList<TGetResult>>(res));
    }

    public virtual async Task<Result<IReadOnlyList<TGetProjectedResult>>> GetBySpecAsync<TGetProjectedResult>(ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetBySpecAsync(specification)!;
        return res.Count is 0 ? Result<IReadOnlyList<TGetProjectedResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetProjectedResult>>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetAllAsync<TGetResult>(bool shouldProject = false) where TGetResult : class
    {
        IReadOnlyList<TGetResult> res;
        if (shouldProject) res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetAllAsync<TGetResult>()!;
        else res = this.Mapper.Map<IReadOnlyList<TGetResult>>(await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
            ?.GetAllAsync()!);

        return res.Count is 0 ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError()) : Result<IReadOnlyList<TGetResult>>.FromSuccess(res);
    }

    public virtual async Task<Result<long>> LongCountAsync(ISpecification<TEntity>? specification = null)
    {
        var res = await this.UnitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()?.LongCountAsync(specification)!;
        return Result<long>.FromSuccess(res);
    }
}