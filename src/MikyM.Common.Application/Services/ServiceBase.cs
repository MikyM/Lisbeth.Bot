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

using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace MikyM.Common.Application.Services
{
    public abstract class ServiceBase<TContext> : IServiceBase<TContext> where TContext : DbContext
    {
        protected readonly IMapper Mapper;
        protected readonly IUnitOfWork<TContext> UnitOfWork;
        private bool _disposed;

        protected ServiceBase(IMapper mapper, IUnitOfWork<TContext> uof)
        {
            Mapper = mapper;
            UnitOfWork = uof;
        }

        public virtual async Task<int> CommitAsync()
        {
            return await UnitOfWork.CommitAsync();
        }

        public virtual async Task RollbackAsync()
        {
            await UnitOfWork.RollbackAsync();
        }

        public virtual async Task BeginTransactionAsync()
        {
            await UnitOfWork.UseTransaction();
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

            if (disposing) UnitOfWork?.Dispose();

            _disposed = true;
        }
    }
}