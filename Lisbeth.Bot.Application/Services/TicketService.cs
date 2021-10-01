using AutoMapper;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
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
    }
}
