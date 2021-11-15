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

using AutoMapper;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ban;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class BanService : CrudService<Ban, LisbethBotDbContext>, IBanService
{
    public BanService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
    {
    }

    public async Task<Result<(long Id, Ban? FoundEntity)>> AddOrExtendAsync(BanReqDto req, bool shouldSave = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var result = await base.GetSingleBySpecAsync(new ActiveBanSpec(req.TargetUserId, req.GuildId ));

        if (!result.IsDefined())
        {
            var partial = await base.AddAsync(req, shouldSave);
            return Result<(long Id, Ban? FoundEntity)>.FromSuccess((partial.Entity, null));
        }

        if (result.Entity.AppliedUntil > req.AppliedUntil) return (result.Entity.Id, result.Entity);

        var shallowCopy = result.Entity.ShallowCopy();

        base.BeginUpdate(result.Entity);
        result.Entity.AppliedById = req.RequestedOnBehalfOfId;
        result.Entity.AppliedUntil = req.AppliedUntil;
        result.Entity.Reason = req.Reason;

        if (shouldSave) await base.CommitAsync();

        return Result<(long Id, Ban? FoundEntity)>.FromSuccess((result.Entity.Id, shallowCopy));
    }

    public async Task<Result<Ban>> DisableAsync(BanDisableReqDto req, bool shouldSave = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var result = await base.GetSingleBySpecAsync(new ActiveBanSpec(
            req.TargetUserId ?? throw new InvalidOperationException("Id was null, validate the request first"),
            req.GuildId ?? throw new InvalidOperationException("Id was null, validate the request first")));
        if (!result.IsDefined()) return Result<Ban>.FromError(new NotFoundError(), result);

        base.BeginUpdate(result.Entity);
        result.Entity.IsDisabled = true;
        result.Entity.LiftedOn = DateTime.UtcNow;
        result.Entity.LiftedById = req.RequestedOnBehalfOfId;

        if (shouldSave) await base.CommitAsync();

        return Result<Ban>.FromSuccess(result.Entity);
    }
}