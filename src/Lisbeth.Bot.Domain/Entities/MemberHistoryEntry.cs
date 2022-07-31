using System;
using System.Collections.Generic;
using System.Linq;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities;

public class MemberHistoryEntry : SnowflakeEntity, IDisableableEntity
{
    private HashSet<ServerBoosterHistoryEntry>? _serverBoosterHistoryEntries = null;

    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }

    public IEnumerable<ServerBoosterHistoryEntry>? ServerBoosterHistoryEntries =>
        _serverBoosterHistoryEntries?.AsEnumerable();
    
    public bool IsDisabled { get; set; }
    public string Username { get; set; } = null!;
    public DateTime AccountCreated { get; set; }
    
    public void AddServerBoosterHistoryEntry(ServerBoosterHistoryEntry entry)
    {
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(entry);
    }
    
    public void AddServerBoosterHistoryEntry(ulong userId, string username, DateTime? dateOverride = null)
    {
        var date = dateOverride ?? DateTime.UtcNow;
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(new ServerBoosterHistoryEntry { UserId = userId, GuildId = this.GuildId, CreatedAt = date, Username = username } );
    }

}
