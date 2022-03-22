using System.Linq.Expressions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Common.Utilities.Results;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordEmbedConfiguratorService<T> where T : SnowflakeDiscordEntity
{
    Task<Result<DiscordEmbed>> ConfigureAsync<TEmbedProperty>(InteractionContext ctx,
        Expression<Func<T, TEmbedProperty?>> embedToConfigure, Expression<Func<T, long?>> embedIdProperty, string? idOrName = null) where TEmbedProperty : EmbedConfig;
}
