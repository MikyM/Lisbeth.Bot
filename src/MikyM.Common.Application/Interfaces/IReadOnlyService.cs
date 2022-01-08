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

using MikyM.Common.Application.Results;
using MikyM.Common.DataAccessLayer.Specifications;

namespace MikyM.Common.Application.Interfaces;

public interface IReadOnlyDataService<TEntity, TContext> : IDataServiceBase<TContext>
    where TEntity : AggregateRootEntity where TContext : DbContext
{
    Task<Result<TEntity>> GetAsync(params object[] keyValues);

    Task<Result<TGetResult>> GetAsync<TGetResult>(bool shouldProject = false, params object[] keyValues) where TGetResult : class;

    Task<Result<TEntity>> GetSingleBySpecAsync(ISpecification<TEntity> specification);

    Task<Result<TGetResult>> GetSingleBySpecAsync<TGetResult>(ISpecification<TEntity> specification)
        where TGetResult : class;

    Task<Result<TGetProjectedResult>> GetSingleBySpecAsync<TGetProjectedResult>(
        ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class;

    Task<Result<IReadOnlyList<TEntity>>> GetBySpecAsync(ISpecification<TEntity> specification);

    Task<Result<IReadOnlyList<TGetResult>>> GetBySpecAsync<TGetResult>(ISpecification<TEntity> specification)
        where TGetResult : class;

    Task<Result<IReadOnlyList<TGetProjectedResult>>> GetBySpecAsync<TGetProjectedResult>(
        ISpecification<TEntity, TGetProjectedResult> specification) where TGetProjectedResult : class;

    Task<Result<IReadOnlyList<TGetResult>>> GetAllAsync<TGetResult>(bool shouldProject = false)
        where TGetResult : class;

    Task<Result<IReadOnlyList<TEntity>>> GetAllAsync();

    Task<Result<long>> LongCountAsync(ISpecification<TEntity>? specification = null);
}