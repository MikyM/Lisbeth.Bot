using System;
using System.Collections.Generic;
using System.Linq;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities;

public class MemberHistoryEntry : SnowflakeEntity, IDisableableEntity
{
    private readonly HashSet<ServerBoosterHistoryEntry>? _serverBoosterHistoryEntries = null;

    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }

    public IEnumerable<ServerBoosterHistoryEntry>? ServerBoosterHistoryEntries =>
        _serverBoosterHistoryEntries?.AsEnumerable();
    
    public bool IsDisabled { get; set; }
    public string Username { get; set; } = null!;
    public DateTime AccountCreated { get; set; }
}
