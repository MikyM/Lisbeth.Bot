namespace Lisbeth.Bot.Domain.Entities;

public class GuildServerBooster
{
    public ulong ServerBoosterId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }
    public ServerBooster? ServerBooster { get; set; }
}