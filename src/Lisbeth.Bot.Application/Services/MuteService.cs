﻿// This file is part of Lisbeth.Bot project
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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services
{
    public class MuteService : CrudService<Mute, LisbethBotDbContext>, IMuteService
    {
        public MuteService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<(long Id, Mute FoundEntity)> AddOrExtendAsync(MuteReqDto req, bool shouldSave = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await _unitOfWork.GetRepository<Repository<Mute>>()
                .GetBySpecificationsAsync(new Specifications<Mute>(x =>
                    x.UserId == req.TargetUserId && x.GuildId == req.GuildId && !x.IsDisabled));

            var entity = res.FirstOrDefault();
            if (entity is null) return (await base.AddAsync(req, shouldSave), null);

            if (entity.AppliedUntil > req.AppliedUntil) return (entity.Id, entity);

            var shallowCopy = entity.ShallowCopy();

            base.BeginUpdate(entity);
            entity.AppliedById = req.RequestedOnBehalfOfId;
            entity.AppliedUntil = req.AppliedUntil;
            entity.Reason = req.Reason;

            if (shouldSave) await base.CommitAsync();

            return (entity.Id, shallowCopy);
        }

        public async Task<Mute> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            var res = await base.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x =>
                    x.UserId == entry.TargetUserId && x.GuildId == entry.GuildId && !x.IsDisabled));

            var entity = res.FirstOrDefault();
            if (entity is null) return null;

            base.BeginUpdate(entity);
            entity.IsDisabled = true;
            entity.LiftedOn = DateTimeOffset.UtcNow;
            entity.LiftedById = entry.RequestedOnBehalfOfId;

            if (shouldSave) await base.CommitAsync();

            return entity;
        }
    }
}