using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities;

public class ServerBoosterHistoryEntry : SnowflakeEntity, IDisableableEntity
{
    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }
    public MemberHistoryEntry? MemberHistoryEntry { get; set; }
    public bool IsDisabled { get; set; }
    public string Username { get; set; } = null!;
}
