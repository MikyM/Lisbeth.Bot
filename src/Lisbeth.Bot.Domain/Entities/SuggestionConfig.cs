using Lisbeth.Bot.Domain.Entities.Base;
namespace Lisbeth.Bot.Domain.Entities;

public class SuggestionConfig : SnowflakeDiscordEntity
{
    public Guild? Guild { get; set; }
    public ulong SuggestionChannelId { get; set; }
    public bool ShouldCreateThreads { get; set; }
    public bool ShouldAddVoteReactions { get; set; }
}
