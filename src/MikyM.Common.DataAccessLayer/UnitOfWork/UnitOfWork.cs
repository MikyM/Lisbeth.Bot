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

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace MikyM.Common.DataAccessLayer.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : AuditableDbContext
{
    private readonly ILifetimeScope _lifetimeScope;

    // To detect redundant calls
    private bool _disposed;
    // ReSharper disable once InconsistentNaming
    private ConcurrentDictionary<string, IBaseRepository>? _repositories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TContext context, ILifetimeScope lifetimeScope)
    {
        Context = context;
        _lifetimeScope = lifetimeScope;
    }

    public TContext Context { get; }

    public async Task UseTransaction()
    {
        _transaction ??= await Context.Database.BeginTransactionAsync();
    }

    public TRepository GetRepository<TRepository>() where TRepository : class, IBaseRepository
    {
        _repositories ??= new ConcurrentDictionary<string, IBaseRepository>();

        var type = typeof(TRepository);
        string name = type.FullName ?? throw new InvalidOperationException();

        if (_repositories.TryGetValue(name, out var repository)) return (TRepository) repository;

        if (_repositories.TryAdd(name,
                _lifetimeScope.Resolve<TRepository>(new ResolvedParameter(
                    (pi, _) => pi.ParameterType.IsAssignableTo(typeof(DbContext)), (_, _) => Context))))
            return (TRepository)_repositories[name];

        if (_repositories.TryGetValue(name, out repository)) return (TRepository) repository;

        throw new InvalidOperationException(
            $"Repository of type {name} couldn't be added to and/or retrieved.");
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

    public async Task<int> CommitAsync(string? userId)
    {
        int result = await Context.SaveChangesAsync(userId);
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
            Context.Dispose();
            _transaction?.Dispose();
        }

        _repositories = null;

        _disposed = true;
    }
}