using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MikyM.Common.Application.Interfaces
{
    public interface IServiceBase<TContext> : IDisposable where TContext : DbContext
    {
        Task<int> CommitAsync();
        Task RollbackAsync();
        Task BeginTransactionAsync();
    }
}
