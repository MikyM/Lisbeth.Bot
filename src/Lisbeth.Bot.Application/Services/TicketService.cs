// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
            
            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();
            if (ticket is null) return null;

            BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTime.UtcNow;
            ticket.IsDisabled = true;

            return ticket;
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req, Ticket ticket)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            BeginUpdate(ticket);
            ticket.ClosedById = req.RequestedById;
            ticket.ClosedOn = DateTime.UtcNow;
            ticket.IsDisabled = true;
            ticket.MessageCloseId = req.ClosedMessageId;

            await CommitAsync();

            return ticket;
        }

        public async Task<Ticket> OpenAsync(TicketOpenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId));

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
            ticket.ReopenedOn = DateTime.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await CommitAsync();

            return ticket;
        }

        public async Task<Ticket> ReopenAsync(TicketReopenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId, true));
            var ticket = res.FirstOrDefault();

            if (ticket is null) return null;

            BeginUpdate(ticket);
            ticket.ReopenedById = req.RequestedById;
            ticket.ReopenedOn = DateTime.UtcNow;
            ticket.IsDisabled = false;
            ticket.MessageCloseId = null;
            ticket.MessageReopenId = req.ReopenMessageId;

            await CommitAsync();

            return ticket;
        }
    }
}
