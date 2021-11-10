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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class TicketService : CrudService<Ticket, LisbethBotDbContext>, ITicketService
    {
        public TicketService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<Result> CheckForDeletedTicketChannelAsync(ulong channelId, ulong guildId,
            ulong requestedOnBehalfOfId)
        {
            var res = await base.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, null, guildId, channelId, null, false, 1));

            if (!res.IsSuccess) return Result.FromSuccess();

            var req = new TicketCloseReqDto(res.Entity.Id, res.Entity.UserId, res.Entity.GuildId, res.Entity.ChannelId,
                requestedOnBehalfOfId);
            await CloseAsync(req, res.Entity);

            return Result.FromSuccess();
        }

        public async Task<Result<Ticket>> CloseAsync(TicketCloseReqDto req, bool shouldSave = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId));

            if (!res.IsSuccess) return Result<Ticket>.FromError(res);

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

            var res = await base.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, req.RequestedOnBehalfOfId, req.GuildId));

            if (res.IsSuccess) return new InvalidOperationError("User already has an opened ticket in this guild");

            var id = await base.AddAsync(req, true);

            return await base.GetAsync<Ticket>(id.Entity);
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

            var res = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId, true));

            if (!res.IsSuccess) return Result<Ticket>.FromError(res);

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

            ticket.AddedRoleIds = discordRoles;

            if (shouldSave) await base.CommitAsync();

            return Result.FromSuccess();
        }

        public async Task<Result> CheckAndSetPrivacyAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            var isPrivateRes = await IsTicketPrivateAsync(ticket, guild);

            if (isPrivateRes.IsSuccess && isPrivateRes.Entity == ticket.IsPrivate) return Result.FromSuccess();

            ticket.IsPrivate = isPrivateRes.Entity;

            await base.CommitAsync();

            return Result.FromSuccess();
        }

        public async Task<Result<bool>> IsTicketPrivateAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            if (ticket.AddedUserIds?.Count == 0) return new InvalidOperationError("User list was empty");

            if (ticket.AddedUserIds is null) return true;

            foreach (var id in ticket.AddedUserIds)
            {
                try
                {
                    var member = await guild.GetMemberAsync(id);
                    if (!member.Permissions.HasPermission(Permissions.Administrator) && member.Id != ticket.UserId)
                        return false;
                }
                catch (Exception)
                {
                    continue;
                }

                await Task.Delay(500);
            }

            return true;
        }
    }
}