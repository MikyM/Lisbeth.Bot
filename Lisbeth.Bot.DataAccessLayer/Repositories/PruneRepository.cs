using Lisbeth.Bot.DataAccessLayer.Repositories.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.DataAccessLayer.Repositories
{
    public class PruneRepository : Repository<Prune>, IPruneRepository
    {
        public PruneRepository(LisbethBotDbContext ctx) : base(ctx)
        {
        }
    }
}
