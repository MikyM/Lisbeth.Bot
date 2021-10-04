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

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services
{
    public class TicketService : CrudService<Ticket, LisbethBotDbContext>, ITicketService
    {
        public TicketService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<Ticket> CloseAsync(TicketCloseReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            
            var res = await GetBySpecificationsAsync<Ticket>(new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, null, 1));

            var ticket = res.FirstOrDefault();
            if (ticket is null) return null;

            BeginUpdate(ticket);
            ticket.ClosedBy = req.RequestedById;
            ticket.ClosedOn = DateTime.UtcNow;
            ticket.IsDisabled = true;

            return ticket;
        }
    }
}
