using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Interfaces
{
    public interface IMuteService : ICrudService<Mute, LisbethBotDbContext>
    {
        Task<long> AddOrUpdateAsync(MuteReqDto entry, bool shouldSave = false);
        Task<bool> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false);
    }
}
