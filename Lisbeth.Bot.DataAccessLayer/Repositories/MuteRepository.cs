using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer.Repositories.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Repositories;

namespace Lisbeth.Bot.DataAccessLayer.Repositories
{
    public class MuteRepository : Repository<Mute>, IMuteRepository
    {
        public MuteRepository(LisbethBotDbContext ctx) : base(ctx) { }

        public void Disable(Mute entity, ulong userId)
        {
            entity.LiftedById = userId;
            base.Disable(entity);
        }
    }
}
