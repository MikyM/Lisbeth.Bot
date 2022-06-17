using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities;

public class GuildServerBooster : SnowflakeEntity
{
    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }
    public ServerBooster? ServerBooster { get; set; }

    public GuildServerBooster()
    {
    }

    public GuildServerBooster(ulong userId, ulong guildId)
    {
        UserId = userId;
        GuildId = guildId;
    }
}
