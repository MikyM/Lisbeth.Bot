using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordTicketService : IDiscordTicketService
    {
        public Task<DiscordEmbed> CloseTicketAsync(TicketCloseReqDto req, DiscordInteraction intr = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<DiscordEmbed> CloseTicketAsync(TicketCloseReqDto req, DiscordChannel channel = null, DiscordUser user = null,
            DiscordGuild guild = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
