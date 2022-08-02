using Lisbeth.Bot.Domain.Entities.Base;
namespace Lisbeth.Bot.Domain.Entities;

public class Suggestion : SnowflakeDiscordEntity
{
    public Guild? Guild { get; set; }
    public ulong UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Text { get; set; } = null!;
    public ulong? ThreadId { get; set; }
    public ulong MessageId { get; set; }
}
