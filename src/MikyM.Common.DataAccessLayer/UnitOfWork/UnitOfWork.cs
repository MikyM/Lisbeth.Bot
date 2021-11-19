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
using Microsoft.EntityFrameworkCore.Storage;
using MikyM.Common.DataAccessLayer.Helpers;

namespace MikyM.Common.DataAccessLayer.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    private readonly ISpecificationEvaluator _specificationEvaluator;

    // To detect redundant calls
    private bool _disposed;
    // ReSharper disable once InconsistentNaming
    private Dictionary<string, IBaseRepository>? _repositories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TContext context, ISpecificationEvaluator specificationEvaluator)
    {
        Context = context;
        _specificationEvaluator = specificationEvaluator;
    }

    public TContext Context { get; }

    public async Task UseTransaction()
    {
        _transaction ??= await Context.Database.BeginTransactionAsync();
    }

    public TRepository? GetRepository<TRepository>() where TRepository : IBaseRepository
    {
        _repositories ??= new Dictionary<string, IBaseRepository>();

        var type = typeof(TRepository);
        string name = type.FullName ?? throw new InvalidOperationException();

        if (_repositories.TryGetValue(name, out var repository)) return (TRepository) repository;

        Type? concrete;

        if (UoFCache.CachedTypes.TryGetValue(name, out concrete) && concrete is not null && type.IsAssignableFrom(concrete))
        {
            string? concreteName = concrete.FullName ?? throw new InvalidOperationException();

            if (_repositories.TryGetValue(concreteName, out var concreteRepo)) return (TRepository) concreteRepo;

            if (_repositories.TryAdd(concreteName,
                    (TRepository) Activator.CreateInstance(concrete, Context, _specificationEvaluator)! ?? throw new InvalidOperationException()))
                return (TRepository) _repositories[concreteName];
            throw new ArgumentException(
                $"Concrete repository of type {concreteName} couldn't be added to and/or retrieved from cache.");
        }

        if (_repositories.TryAdd(name,
                (TRepository) Activator.CreateInstance(type, Context, _specificationEvaluator)! ?? throw new InvalidOperationException()))
            return (TRepository) _repositories[name]!;

        throw new ArgumentException(
            $"Concrete repository of type {name} couldn't be added to and/or retrieved from cache.");
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null) await _transaction.RollbackAsync();
    }

    public async Task<int> CommitAsync()
    {
        int result = await Context.SaveChangesAsync();
        if (_transaction is not null) await _transaction.CommitAsync();
        return result;
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Context?.Dispose();
            _transaction?.Dispose();
        }

        _repositories = null;

        _disposed = true;
    }
}