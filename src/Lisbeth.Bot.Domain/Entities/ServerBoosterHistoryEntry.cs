﻿namespace Lisbeth.Bot.Domain.Entities;

public class ServerBoosterHistoryEntry : LisbethEntity
{
    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public Guild? Guild { get; set; }
    public MemberHistoryEntry? MemberHistoryEntry { get; set; }
    public long MemberHistoryEntryId { get; set; }
    public string Username { get; set; } = null!;
}
