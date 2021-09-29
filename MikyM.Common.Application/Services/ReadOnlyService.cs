using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikyM.Common.Application.Services
{
    public class ReadOnlyService<TEntity, TContext> : ServiceBase<TContext>, IReadOnlyService<TEntity, TContext>
        where TEntity : AggregateRootEntity where TContext : DbContext
    {
        public ReadOnlyService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
        {
        }

        public virtual async Task<TGetResult> GetAsync<TGetResult>(long id) where TGetResult : class
        {
            return _mapper.Map<TGetResult>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>().GetAsync(id));
        }

        public virtual async Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(
            ISpecifications<TEntity> specifications = null) where TGetResult : class
        {
            return _mapper.Map<IReadOnlyList<TGetResult>>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                .GetBySpecificationsAsync(specifications));
        }

        public virtual async Task<IReadOnlyList<TGetResult>> GetBySpecificationsAsync<TGetResult>(
            PaginationFilterDto filter, ISpecifications<TEntity> specifications = null) where TGetResult : class
        {
            return _mapper.Map<IReadOnlyList<TGetResult>>(await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>()
                .GetBySpecificationsAsync(_mapper.Map<PaginationFilter>(filter), specifications));
        }

        public virtual async Task<long> LongCountAsync(ISpecifications<TEntity> specifications = null)
        {
            return await _unitOfWork.GetRepository<ReadOnlyRepository<TEntity>>().LongCountAsync(specifications);
        }
    }
}
