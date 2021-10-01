using AutoMapper;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services
{
    public class GuildService : CrudService<Guild, LisbethBotDbContext>, IGuildService
    {
        public GuildService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> ctx) : base(mapper, ctx)
        {
            
        }
    }
}
