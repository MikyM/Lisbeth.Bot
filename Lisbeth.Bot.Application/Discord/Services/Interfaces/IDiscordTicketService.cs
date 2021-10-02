using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTicketService
    {
        Task<DiscordEmbed> CloseTicketAsync(TicketCloseReqDto req, DiscordInteraction intr = null);
        Task<DiscordEmbed> CloseTicketAsync(TicketCloseReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
    }
}
