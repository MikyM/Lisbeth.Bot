using AutoMapper;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.Application.Services
{
    public class MuteService : CrudService<Mute, LisbethBotDbContext>, IMuteService
    {
        public MuteService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof) { }

        public async Task<long> AddOrUpdateAsync(MuteReqDto entry, bool shouldSave = false)
        {
            var mutes = await _unitOfWork.GetRepository<Repository<Mute>>()
                .GetBySpecificationsAsync(new Specifications<Mute>(x => x.UserId == entry.UserId && !x.IsDisabled));
            var mute = mutes.FirstOrDefault();

            if (mute is null) return await base.AddAsync(entry, shouldSave);

            mute.Extend(entry.MutedById, entry.MutedUntil, entry.Reason);
            await base.UpdateAsync(entry, shouldSave);
            return mute.Id;
        }

        public async Task<bool> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            var entities = await _unitOfWork.GetRepository<MuteRepository>()
                .GetBySpecificationsAsync(new Specifications<Mute>(x => x.UserId == entry.UserId && !x.IsDisabled));

            if (entities is null || !entities.Any())
                return false;

            _unitOfWork.GetRepository<MuteRepository>()
                .Disable(_mapper.Map<Mute>(entry), entry.LiftedById);

            if (shouldSave) await CommitAsync();
            return true;
        }
    }
}
