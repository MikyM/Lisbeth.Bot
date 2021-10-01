using Lisbeth.Bot.DataAccessLayer.Repositories.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.DataAccessLayer.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(LisbethBotDbContext ctx) : base(ctx)
        {
            
        }
    }
}
