using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Services.Interfaces
{
    public interface IPruneService : ICrudService<Prune, LisbethBotDbContext>
    {
        
    }
}
