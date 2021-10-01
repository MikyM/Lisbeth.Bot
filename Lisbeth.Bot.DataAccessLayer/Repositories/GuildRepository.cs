using Lisbeth.Bot.DataAccessLayer.Repositories.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.DataAccessLayer.Repositories
{
    public class GuildRepository : Repository<Guild>, IGuildRepository
    {
        public GuildRepository(LisbethBotDbContext ctx) : base(ctx)
        {
            
        }
    }
}
