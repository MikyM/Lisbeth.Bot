using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Services.Interfaces
{
    public interface IBanService : ICrudService<Ban, LisbethBotDbContext>
    {
        Task<(long Id, Ban FoundEntity)> AddOrExtendAsync(BanReqDto req, bool shouldSave = false);
        Task<Ban> DisableAsync(BanDisableReqDto entry, bool shouldSave = false);
    }
}
