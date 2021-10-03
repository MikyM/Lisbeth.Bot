using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.ChatExport
{
    public interface IDiscordChatExportService
    {
        public Task<DiscordEmbed> ExportToHtmlAsync(TicketExportReqDto req, DiscordUser triggerUser = null);
    }
}
