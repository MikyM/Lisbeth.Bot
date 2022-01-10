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

namespace MikyM.Common.Application.Interfaces;

public interface ICrudService<TEntity, TContext> : IReadOnlyDataService<TEntity, TContext>
    where TEntity : AggregateRootEntity where TContext : DbContext
{
    Task<Result<long>> AddAsync<TPost>(TPost entry, bool shouldSave = false, string? userId = null) where TPost : class;

    Task<Result<IEnumerable<long>>> AddRangeAsync<TPost>(IEnumerable<TPost> entries, bool shouldSave = false,
        string? userId = null) where TPost : class;

    Result BeginUpdate<TPatch>(TPatch entry, bool shouldSwapAttached = false) where TPatch : class;

    Result BeginUpdateRange<TPatch>(IEnumerable<TPatch> entries, bool shouldSwapAttached = false) where TPatch : class;

    Task<Result> DeleteAsync<TDelete>(TDelete entry, bool shouldSave = false, string? userId = null)
        where TDelete : class;

    Task<Result> DeleteAsync(long id, bool shouldSave = false, string? userId = null);

    Task<Result> DeleteRangeAsync<TDelete>(IEnumerable<TDelete> entries, bool shouldSave = false, string? userId = null)
        where TDelete : class;

    Task<Result> DeleteRangeAsync(IEnumerable<long> ids, bool shouldSave = false, string? userId = null);

    Task<Result> DisableAsync<TDisable>(TDisable entry, bool shouldSave = false, string? userId = null)
        where TDisable : class;

    Task<Result> DisableAsync(long id, bool shouldSave = false, string? userId = null);

    Task<Result> DisableRangeAsync<TDisable>(IEnumerable<TDisable> entries, bool shouldSave = false,
        string? userId = null) where TDisable : class;

    Task<Result> DisableRangeAsync(IEnumerable<long> ids, bool shouldSave = false, string? userId = null);
}