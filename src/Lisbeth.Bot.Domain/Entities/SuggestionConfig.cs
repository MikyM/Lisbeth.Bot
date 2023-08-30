namespace Lisbeth.Bot.Domain.Entities;

public class SuggestionConfig : LisbethDiscordEntity
{
    public Guild? Guild { get; set; }
    public ulong SuggestionChannelId { get; set; }
    public bool ShouldCreateThreads { get; set; }
    public bool ShouldAddVoteReactions { get; set; }
}
