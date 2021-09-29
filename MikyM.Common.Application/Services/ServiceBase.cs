using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace MikyM.Common.Application.Services
{
    public abstract class ServiceBase<TContext> : IServiceBase<TContext> where TContext : DbContext
    {
        protected readonly IMapper _mapper;
        protected readonly IUnitOfWork<TContext> _unitOfWork;

        protected ServiceBase(IMapper mapper, IUnitOfWork<TContext> uof)
        {
            _mapper = mapper;
            _unitOfWork = uof;
        }

        public virtual async Task<int> CommitAsync()
        {
            return await _unitOfWork.CommitAsync();
        }

        public virtual async Task RollbackAsync()
        {
            await _unitOfWork.RollbackAsync();
        }

        public virtual async Task BeginTransactionAsync()
        {
            await _unitOfWork.UseTransaction();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
        }
    }
}
