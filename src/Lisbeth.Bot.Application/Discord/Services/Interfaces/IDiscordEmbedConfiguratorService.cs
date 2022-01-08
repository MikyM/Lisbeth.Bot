using System.Linq.Expressions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordEmbedConfiguratorService<T> : IServiceBase where T : SnowflakeDiscordEntity
{
    Task<Result<DiscordEmbed>> ConfigureAsync<TEmbedProperty>(InteractionContext ctx,
        Expression<Func<T, TEmbedProperty?>> embedToConfigure, Expression<Func<T, long?>> embedIdProperty, string? idOrName = null) where TEmbedProperty : EmbedConfig;
}
