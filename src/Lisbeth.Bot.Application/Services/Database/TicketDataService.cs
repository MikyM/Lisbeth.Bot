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

using System.Collections.Generic;
using AutoMapper;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class TicketDataService : CrudDataService<Ticket, LisbethBotDbContext>, ITicketDataService
{
    public TicketDataService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
    {
    }

    public async Task<Result> CheckForDeletedTicketChannelAsync(ulong channelId, ulong guildId,
        ulong requestedOnBehalfOfId)
    {
        var res = await base.GetSingleBySpecAsync<Ticket>(
            new TicketBaseGetSpecifications(null, null, guildId, channelId, null, false, 1));

        if (!res.IsDefined()) return Result.FromSuccess();

        var req = new TicketCloseReqDto(res.Entity.UserId, res.Entity.GuildId, res.Entity.ChannelId,
            requestedOnBehalfOfId);
        await CloseAsync(req, res.Entity);

        return Result.FromSuccess();
    }

    public async Task<Result<Ticket>> CloseAsync(TicketCloseReqDto req, bool shouldSave = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(null, req.OwnerId,
            req.GuildId, req.ChannelId));

        if (!res.IsDefined()) return Result<Ticket>.FromError(res);

        base.BeginUpdate(res.Entity);
        res.Entity.ClosedById = req.RequestedOnBehalfOfId;
        res.Entity.ClosedOn = DateTime.UtcNow;
        res.Entity.IsDisabled = true;

        if (shouldSave) await base.CommitAsync();

        return res.Entity;
    }

    public async Task<Result<Ticket>> CloseAsync(TicketCloseReqDto req, Ticket ticket)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        base.BeginUpdate(ticket);
        ticket.ClosedById = req.RequestedOnBehalfOfId;
        ticket.ClosedOn = DateTime.UtcNow;
        ticket.IsDisabled = true;
        ticket.MessageCloseId = req.ClosedMessageId;

        await base.CommitAsync();

        return ticket;
    }

    public async Task<Result<Ticket>> OpenAsync(TicketOpenReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await base.GetBySpecAsync(new ActiveTicketByUserIdSpec(req.RequestedOnBehalfOfId));

        if (res.IsDefined() && res.Entity.Count != 0) return new InvalidOperationError("User already has an opened ticket in this guild");

        var id = await base.AddAsync(req, true);

        return await base.GetAsync(id.Entity);
    }

    public async Task<Result<Ticket>> ReopenAsync(TicketReopenReqDto req, Ticket ticket)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        base.BeginUpdate(ticket);
        ticket.ReopenedById = req.RequestedOnBehalfOfId;
        ticket.ReopenedOn = DateTime.UtcNow;
        ticket.IsDisabled = false;
        ticket.MessageCloseId = null;
        ticket.MessageReopenId = req.ReopenMessageId;

        await base.CommitAsync();

        return ticket;
    }

    public async Task<Result<Ticket>> ReopenAsync(TicketReopenReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(null, req.OwnerId,
            req.GuildId, req.ChannelId, null, true));

        if (!res.IsDefined()) return Result<Ticket>.FromError(res);

        base.BeginUpdate(res.Entity);
        res.Entity.ReopenedById = req.RequestedOnBehalfOfId;
        res.Entity.ReopenedOn = DateTime.UtcNow;
        res.Entity.IsDisabled = false;
        res.Entity.MessageCloseId = null;
        res.Entity.MessageReopenId = req.ReopenMessageId;

        await base.CommitAsync();

        return res.Entity;
    }

    public async Task<Result> SetAddedUsersAsync(Ticket ticket, IEnumerable<ulong> userIds, bool shouldSave = false)
    {
        if (userIds is null) throw new ArgumentNullException(nameof(userIds));
        if (ticket is null) throw new ArgumentNullException(nameof(ticket));
        var discordMembers = userIds.ToList();

        if (!discordMembers.Any()) return Result.FromSuccess();

        base.BeginUpdate(ticket);
        ticket.AddedUserIds = discordMembers;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> SetAddedRolesAsync(Ticket ticket, IEnumerable<ulong> roleIds, bool shouldSave = false)
    {
        if (roleIds is null) throw new ArgumentNullException(nameof(roleIds));
        if (ticket is null) throw new ArgumentNullException(nameof(ticket));
        var discordRoles = roleIds.ToList();
        if (!discordRoles.Any()) return Result.FromSuccess();

        base.BeginUpdate(ticket);
        ticket.AddedRoleIds = discordRoles;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> SetPrivacyAsync(Ticket ticket, bool isPrivate, bool shouldSave = false)
    {
        if (ticket is null) throw new ArgumentNullException(nameof(ticket));

        base.BeginUpdate(ticket);
        ticket.IsPrivate = isPrivate;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }
}
