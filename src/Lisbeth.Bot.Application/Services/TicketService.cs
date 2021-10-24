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
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services
{
    [UsedImplicitly]
    public class TicketService : CrudService<Ticket, LisbethBotDbContext>, ITicketService
    {
        public TicketService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();
            if (ticket is null) return null;

            BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTimeOffset.UtcNow;
            ticket.IsDisabled = true;

            return ticket;
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req, Ticket ticket)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTimeOffset.UtcNow;
            ticket.IsDisabled = true;
            ticket.MessageCloseId = req.ClosedMessageId;

            await CommitAsync();

            return ticket;
        }

        public async Task<Ticket> OpenAsync(TicketOpenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId));

            var ticket = res.FirstOrDefault();
            if (ticket is not null) return null;

            var id = await AddAsync(req, true);

            return await GetAsync<Ticket>(id);
        }

        public async Task<Ticket> ReopenAsync(TicketReopenReqDto req, Ticket ticket)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            BeginUpdate(ticket);
            ticket.ReopenedById = req.RequestedById;
            ticket.ReopenedOn = DateTimeOffset.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await CommitAsync();

            return ticket;
        }

        public async Task<Ticket> ReopenAsync(TicketReopenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId,
                req.GuildId, req.ChannelId, req.GuildSpecificId, true));
            var ticket = res.FirstOrDefault();

            if (ticket is null) return null;

            BeginUpdate(ticket);
            ticket.ReopenedById = req.RequestedById;
            ticket.ReopenedOn = DateTimeOffset.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await CommitAsync();

            return ticket;
        }

        public async Task SetAddedUsersAsync(Ticket ticket, IEnumerable<ulong> userIds, bool shouldSave = false)
        {
            if (userIds is null) throw new ArgumentNullException(nameof(userIds));
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            var discordMembers = userIds.ToList();
            if (!discordMembers.Any()) return;

            BeginUpdate(ticket);
            ticket.AddedUsers = JsonSerializer.Serialize(discordMembers);

            if (shouldSave) await CommitAsync();
        }

        public async Task SetAddedRolesAsync(Ticket ticket, IEnumerable<ulong> roleIds, bool shouldSave = false)
        {
            if (roleIds is null) throw new ArgumentNullException(nameof(roleIds));
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            var discordRoles = roleIds.ToList();
            if (!discordRoles.Any()) return;

            BeginUpdate(ticket);
            ticket.AddedRoles = JsonSerializer.Serialize(discordRoles);

            if (shouldSave) await CommitAsync();
        }

        public async Task CheckAndSetPrivacyAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            var isPrivate = await IsTicketPrivateAsync(ticket, guild);

            if (isPrivate == ticket.IsPrivate) return;

            BeginUpdate(ticket);
            ticket.IsPrivate = isPrivate;

            await CommitAsync();
        }

        public async Task<bool> IsTicketPrivateAsync(Ticket ticket, DiscordGuild guild)
        {
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));
            if (guild is null) throw new ArgumentNullException(nameof(guild));

            var userList = JsonSerializer.Deserialize<List<ulong>>(ticket.AddedUsers);

            if (userList is null || userList.Count == 0) return false;

            foreach (var id in userList)
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