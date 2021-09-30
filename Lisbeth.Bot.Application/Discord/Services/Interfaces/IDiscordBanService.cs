using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordBanService
    {
        Task<DiscordEmbed> BanAsync(BanReqDto req, ulong logChannelId = 0, InteractionContext ctx = null);
        Task<DiscordEmbed> UnbanAsync(BanDisableReqDto req, ulong logChannelId = 0, InteractionContext ctx = null);
    }
}
