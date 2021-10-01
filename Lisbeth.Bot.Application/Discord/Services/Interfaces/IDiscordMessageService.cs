using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordMessageService
    {
        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, InteractionContext ctx = null, bool isSingleMessageDelete = false);
        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null, bool isSingleMessageDelete = false);
        Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, DiscordChannel channel = null, DiscordGuild guild = null, DiscordUser moderator = null, DiscordUser author = null, DiscordMessage message = null, bool isSingleMessageDelete = false, ulong idToSkip = 0);
    }
}
