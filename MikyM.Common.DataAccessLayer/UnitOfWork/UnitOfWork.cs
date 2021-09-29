using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MikyM.Common.DataAccessLayer.Helpers;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.Domain.Entities;

namespace MikyM.Common.DataAccessLayer.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public TContext Context { get; }
        protected Dictionary<string, IBaseRepository> _repositories;
        private IDbContextTransaction _transaction;

        public UnitOfWork(TContext context)
        {
            Context = context;
        }

        public virtual async Task UseTransaction()
        {
            _transaction ??= await Context.Database.BeginTransactionAsync();
        }

        public virtual TRepository GetRepository<TRepository>() where TRepository : IBaseRepository
        {
            _repositories ??= new();
            var type = typeof(TRepository);
            string name = type.FullName;
            if (_repositories.TryGetValue(name, out var repository)) return (TRepository)repository;

            var concrete =
                UoFCache.CachedTypes.FirstOrDefault(x => type.IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
            if (concrete is not null)
            {
                string concreteName = concrete.FullName;
                if (_repositories.TryGetValue(concreteName, out var concreteRepo)) return (TRepository)concreteRepo;
                if (_repositories.TryAdd(concreteName, (TRepository)Activator.CreateInstance(concrete, Context)))
                    return (TRepository)_repositories[concreteName];
                throw new ArgumentException(
                    $"Concrete repository of type {concreteName} couldn't be added to and/or retrieved from cache.");
            }

            if (_repositories.TryAdd(name, (TRepository)Activator.CreateInstance(type, Context))) return (TRepository)_repositories[name];
            throw new ArgumentException(
                $"Concrete repository of type {name} couldn't be added to and/or retrieved from cache.");
        }

        public virtual async Task RollbackAsync()
        {
            if (_transaction is not null) await _transaction.RollbackAsync();
        }

        public virtual async Task<int> CommitAsync()
        {
            int result = await Context.SaveChangesAsync();
            if (_transaction is not null) await _transaction.CommitAsync();
            return result;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }
    }
}
