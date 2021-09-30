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

namespace Lisbeth.Bot.Application.Services
{
    public class BanService : CrudService<Ban, LisbethBotDbContext>, IBanService
    {
        public BanService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof) { }

        public async Task<(long Id, Ban FoundEntity)> AddOrExtendAsync(BanReqDto req, bool shouldSave = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await _unitOfWork.GetRepository<Repository<Ban>>()
                .GetBySpecificationsAsync(new Specifications<Ban>(x => x.UserId == req.UserId && x.GuildId == req.GuildId && !x.IsDisabled));

            var entity = res.FirstOrDefault();
            if (entity is null) return (await base.AddAsync(req, shouldSave), null);

            if (entity.BannedUntil > req.BannedUntil) return (entity.Id, entity);

            var shallowCopy = entity.ShallowCopy();

            base.BeginUpdate(entity);
            entity.BannedById = req.BannedById;
            entity.BannedOn = DateTime.UtcNow;
            entity.BannedUntil = req.BannedUntil;
            entity.Reason = req.Reason;

            if (shouldSave) await base.CommitAsync();

            return (entity.Id, shallowCopy);
        }

        public async Task<Ban> DisableAsync(BanDisableReqDto entry, bool shouldSave = false)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            var res = await base.GetBySpecificationsAsync<Ban>(
                new Specifications<Ban>(x => x.UserId == entry.UserId && x.GuildId == entry.GuildId && !x.IsDisabled));

            var entity = res.FirstOrDefault();
            if (entity is null) return null;

            base.BeginUpdate(entity);
            entity.IsDisabled = true;
            entity.LiftedOn = DateTime.UtcNow;
            entity.LiftedById = entry.LiftedById;

            if (shouldSave) await base.CommitAsync();

            return entity;

        }
    }
}