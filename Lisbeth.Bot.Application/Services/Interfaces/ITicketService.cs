using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Services.Interfaces
{
    public interface ITicketService : ICrudService<Ticket, LisbethBotDbContext>
    {
        Task<Ticket> CloseAsync(TicketCloseReqDto req);
    }
}
