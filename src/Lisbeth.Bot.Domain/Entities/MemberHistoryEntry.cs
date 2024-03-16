using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities.AuditLogs;

namespace Lisbeth.Bot.Domain.Entities;

public class MemberHistoryEntry : LisbethEntity
{
    private HashSet<ServerBoosterHistoryEntry>? _serverBoosterHistoryEntries = null;

    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }
    public DiscordAuditLogActionType? Punishment { get; set; }
    public string? PunishmentReason { get; set; }
    public string? PunishmentByUsername { get; set; }
    public ulong? PunishmentById { get; set; }

    public IEnumerable<ServerBoosterHistoryEntry>? ServerBoosterHistoryEntries =>
        _serverBoosterHistoryEntries?.AsEnumerable();
    public string Username { get; set; } = null!;
    public DateTime AccountCreated { get; set; }
    
    public void AddServerBoosterHistoryEntry(ServerBoosterHistoryEntry entry)
    {
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(entry);
    }

    public void AddServerBoosterHistoryEntry(ulong userId, string username, long memberHistoryEntryId,
        DateTime? dateOverride = null)
    {
        var date = dateOverride ?? DateTime.UtcNow;
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(new ServerBoosterHistoryEntry
        {
            UserId = userId, GuildId = GuildId, MemberHistoryEntryId = memberHistoryEntryId, CreatedAt = date,
            Username = username
        });
    }
}
