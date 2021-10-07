// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
        private bool _disposed;

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

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) _unitOfWork?.Dispose();

            _disposed = true;
        }
    }
}
