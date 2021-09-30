using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordMuteService
    {
        Task<DiscordEmbed> MuteAsync(MuteReqDto req, ulong logChannelId = 0, InteractionContext ctx = null);
        Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req, ulong logChannelId = 0, InteractionContext ctx = null);
    }
}
