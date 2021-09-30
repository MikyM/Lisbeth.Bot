using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer.Repositories.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.DataAccessLayer.Repositories
{
    public class BanRepository : Repository<Ban>, IBanRepository
    {
        public BanRepository(LisbethBotDbContext ctx) : base(ctx) { }
        }
}
