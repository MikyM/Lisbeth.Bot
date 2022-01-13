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

public class ReadOnlyDataService<TEntity, TContext> : DataServiceBase<TContext>, IReadOnlyDataService<TEntity, TContext>
    where TEntity : AggregateRootEntity where TContext : DbContext
{
    public ReadOnlyDataService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
    {
    }

    public virtual async Task<Result<TGetResult>> GetAsync<TGetResult>(bool shouldProject = false, params object[] keyValues) where TGetResult : class
    {
        var res = await this.GetAsync(keyValues);
        return !res.IsDefined() ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(this.Mapper.Map<TGetResult>(res.Entity));
    }

    public virtual async Task<Result<TEntity>> GetAsync(params object[] keyValues)
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>().GetAsync(keyValues);
        return res is null ? Result<TEntity>.FromError(new NotFoundError()) : Result<TEntity>.FromSuccess(res);
    }

    public virtual async Task<Result<TEntity>> GetSingleBySpecAsync(ISpecification<TEntity> specification)
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetSingleBySpecAsync(specification);
        return res is null ? Result<TEntity>.FromError(new NotFoundError()) : Result<TEntity>.FromSuccess(res);
    }

    public virtual async Task<Result<TGetResult>> GetSingleBySpecAsync<TGetResult>(ISpecification<TEntity> specification) where TGetResult : class
    {
        var res = await this.GetSingleBySpecAsync(specification);
        return !res.IsDefined(out var entity) ? Result<TGetResult>.FromError(new NotFoundError()) : Result<TGetResult>.FromSuccess(this.Mapper.Map<TGetResult>(entity));
    }

    public virtual async Task<Result<TGetProjectedResult>> GetSingleBySpecAsync<TGetProjectedResult>(ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetSingleBySpecAsync(specification);
        return res is null ? Result<TGetProjectedResult>.FromError(new NotFoundError()) : Result<TGetProjectedResult>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetBySpecAsync(ISpecification<TEntity> specification)
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetBySpecAsync(specification);
        return Result<IReadOnlyList<TEntity>>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetBySpecAsync<TGetResult>(
        ISpecification<TEntity> specification) where TGetResult : class
    {
        var res = await this.GetBySpecAsync(specification);
        return res.IsDefined()
            ? Result<IReadOnlyList<TGetResult>>.FromError(new NotFoundError())
            : Result<IReadOnlyList<TGetResult>>.FromSuccess(this.Mapper.Map<IReadOnlyList<TGetResult>>(res.Entity));
    }

    public virtual async Task<Result<IReadOnlyList<TGetProjectedResult>>> GetBySpecAsync<TGetProjectedResult>(ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetBySpecAsync(specification);
        return Result<IReadOnlyList<TGetProjectedResult>>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TGetResult>>> GetAllAsync<TGetResult>(bool shouldProject = false) where TGetResult : class
    {
        IReadOnlyList<TGetResult> res;
        if (shouldProject) res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetAllAsync<TGetResult>();
        else res = this.Mapper.Map<IReadOnlyList<TGetResult>>(await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetAllAsync());

        return Result<IReadOnlyList<TGetResult>>.FromSuccess(res);
    }

    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync()
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>()
            .GetAllAsync();

        return Result<IReadOnlyList<TEntity>>.FromSuccess(res);
    }

    public virtual async Task<Result<long>> LongCountAsync(ISpecification<TEntity>? specification = null)
    {
        var res = await this.UnitOfWork.GetRepository<IReadOnlyRepository<TEntity>>().LongCountAsync(specification);
        return Result<long>.FromSuccess(res);
    }
}