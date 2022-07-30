using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities.Base;

public class SnowflakeDiscordEntity : SnowflakeEntity, IDisableableEntity
{
       public ulong GuildId { get; set; }
       public bool IsDisabled { get; set; }
}
