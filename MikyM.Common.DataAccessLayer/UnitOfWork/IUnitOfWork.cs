using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer.Repositories;
using System;
using System.Threading.Tasks;

namespace MikyM.Common.DataAccessLayer.UnitOfWork
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        TContext Context { get; }
        TRepository GetRepository<TRepository>() where TRepository : IBaseRepository;
        Task<int> CommitAsync();
        Task RollbackAsync();
        Task UseTransaction();
    }
}
