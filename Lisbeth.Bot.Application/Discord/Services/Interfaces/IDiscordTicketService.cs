using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTicketService
    {
        Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
        Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
        Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordInteraction intr = null);
        Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordChannel channel = null, DiscordUser user = null, DiscordGuild guild = null);
    }
}
