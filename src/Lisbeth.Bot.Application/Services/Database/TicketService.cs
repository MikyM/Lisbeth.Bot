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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
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

        public async Task CheckForDeletedTicketChannelAsync(ulong channelId, ulong guildId, ulong requestedOnBehalfOfId)
        {
            var ticket = await base.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, null, guildId, channelId, null, false, 1));

            if (ticket is null) return;

            var req = new TicketCloseReqDto(ticket.Id, ticket.UserId, ticket.GuildId, ticket.ChannelId,
                requestedOnBehalfOfId);
            await CloseAsync(req, ticket);
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req, bool shouldSave = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var ticket = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId));

            if (ticket is null) return null;

            base.BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTime.UtcNow;
            ticket.IsDisabled = true;

            if (shouldSave) await base.CommitAsync();

            return ticket;
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req, Ticket ticket)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            base.BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTime.UtcNow;
            ticket.IsDisabled = true;
            ticket.MessageCloseId = req.ClosedMessageId;

            await base.CommitAsync();

            return ticket;
        }

        public async Task<Ticket> OpenAsync(TicketOpenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var ticket = await base.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId));
            if (ticket is not null) return null;

            var id = await base.AddAsync(req, true);

            return await base.GetAsync<Ticket>(id);
        }

        public async Task<Ticket> ReopenAsync(TicketReopenReqDto req, Ticket ticket)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            base.BeginUpdate(ticket);
            ticket.ReopenedById = req.RequestedById;
            ticket.ReopenedOn = DateTime.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await base.CommitAsync();

            return ticket;
        }

        public async Task<Ticket> ReopenAsync(TicketReopenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var ticket = await base.GetSingleBySpecAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId, true));

            if (ticket is null) return null;

            base.BeginUpdate(ticket);
            ticket.ReopenedById = req.RequestedById;
            ticket.ReopenedOn = DateTime.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await base.CommitAsync();

            return ticket;
        }

        public async Task SetAddedUsersAsync(Ticket ticket, IEnumerable<ulong> userIds, bool shouldSave = false)
        {
            if (userIds is null) throw new ArgumentNullException(nameof(userIds));
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            var discordMembers = userIds.ToList();
            if (!discordMembers.Any()) return;

            ticket.AddedUserIds = discordMembers;

            if (shouldSave) await base.CommitAsync();
        }

        public async Task SetAddedRolesAsync(Ticket ticket, IEnumerable<ulong> roleIds, bool shouldSave = false)
        {
            if (roleIds is null) throw new ArgumentNullException(nameof(roleIds));
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            var discordRoles = roleIds.ToList();
            if (!discordRoles.Any()) return;

            ticket.AddedRoleIds = discordRoles;

            if (shouldSave) await base.CommitAsync();
        }

        public async Task CheckAndSetPrivacyAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            var isPrivate = await IsTicketPrivateAsync(ticket, guild);

            if (isPrivate == ticket.IsPrivate) return;

            ticket.IsPrivate = isPrivate;

            await base.CommitAsync();
        }

        public async Task<bool> IsTicketPrivateAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            if (ticket.AddedUserIds is null || ticket.AddedUserIds.Count == 0) return false;

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