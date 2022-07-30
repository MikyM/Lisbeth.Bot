// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using MikyM.Common.EfCore.DataAccessLayer.UnitOfWork;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class MuteDataService : CrudDataService<Mute, ILisbethBotDbContext>, IMuteDataService
{
    public MuteDataService(IMapper mapper, IUnitOfWork<ILisbethBotDbContext> uof) : base(mapper, uof)
    {
    }

    public async Task<Result<(long Id, Mute? FoundEntity)>> AddOrExtendAsync(MuteApplyReqDto req,
        bool shouldSave = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var result = await base.GetSingleBySpecAsync(new ActiveMutePerGuildAndUserSpec(req.TargetUserId, req.GuildId));
        if (!result.IsDefined())
        {
            var partial = await base.AddAsync(req, shouldSave);
            return Result<(long Id, Mute? FoundEntity)>.FromSuccess((partial.Entity, null));
        }

        var entity = result.Entity;

        if (entity.AppliedUntil > req.AppliedUntil) return Result<(long Id, Mute? FoundEntity)>.FromError(
            new DiscordInvalidOperationError("Existing mute is longer than new one."), result);

        var shallowCopy = entity.ShallowCopy();

        base.BeginUpdate(entity);
        entity.AppliedById = req.RequestedOnBehalfOfId;
        entity.AppliedUntil = req.AppliedUntil;
        entity.Reason = req.Reason;

        if (shouldSave) await base.CommitAsync();

        return (entity.Id, shallowCopy);
    }

    public async Task<Result<Mute>> DisableAsync(MuteRevokeReqDto entry, bool shouldSave = false)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));

        var result =
            await base.GetSingleBySpecAsync(
                new ActiveMutePerGuildAndUserSpec(entry.TargetUserId, entry.GuildId));
        if (!result.IsDefined()) return new NotFoundError();

        var entity = result.Entity;

        base.BeginUpdate(entity);
        entity.IsDisabled = true;
        entity.LiftedOn = DateTime.UtcNow;
        entity.LiftedById = entry.RequestedOnBehalfOfId;

        if (shouldSave) await base.CommitAsync();

        return entity;
    }
}
