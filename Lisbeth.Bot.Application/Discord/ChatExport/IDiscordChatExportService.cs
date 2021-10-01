using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.ChatExport
{
    public interface IDiscordChatExportService
    {
        public Task<DiscordEmbed> ExportToHtml(TicketExportReqDto req, DiscordUser triggerUser = null);
    }
}
