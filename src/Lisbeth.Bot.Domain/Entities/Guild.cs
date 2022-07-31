// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lisbeth.Bot.Domain.Enums;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;
#pragma warning disable CS0649

namespace Lisbeth.Bot.Domain.Entities;

public sealed class Guild : SnowflakeEntity, IDisableableEntity
{
    private readonly HashSet<Ban>? _bans;
    private HashSet<MemberHistoryEntry>? _memberHistoryEntries;
    private HashSet<ServerBoosterHistoryEntry>? _serverBoosterHistoryEntries;
    private readonly HashSet<Mute>? _mutes;
    private readonly HashSet<Prune>? _prunes;
    private readonly HashSet<Reminder>? _reminders;
    private readonly HashSet<RoleMenu>? _roleMenus;
    private readonly HashSet<Tag>? _tags;
    private readonly HashSet<Ticket>? _tickets;
    private readonly HashSet<ChannelMessageFormat>? _channelMessageFormats;

    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong? ReminderChannelId { get; set; }
    public TicketingConfig? TicketingConfig { get; private set; }
    public ModerationConfig? ModerationConfig { get; private set; }
    public string EmbedHexColor { get; set; } = "#26296e";
    public PhishingDetection PhishingDetection { get; set; } = PhishingDetection.Disabled;
    public IEnumerable<Mute>? Mutes => _mutes?.AsEnumerable();
    public IEnumerable<Ban>? Bans => _bans?.AsEnumerable();
    public IEnumerable<Prune>? Prunes => _prunes?.AsEnumerable();
    public IEnumerable<Ticket>? Tickets => _tickets?.AsEnumerable();
    public IEnumerable<MemberHistoryEntry>? MemberHistoryEntries => _memberHistoryEntries?.AsEnumerable();
    public IEnumerable<Reminder>? Reminders => _reminders?.AsEnumerable();
    public IEnumerable<Tag>? Tags => _tags?.AsEnumerable();
    public IEnumerable<RoleMenu>? RoleMenus => _roleMenus?.AsEnumerable();
    public IEnumerable<ChannelMessageFormat>? ChannelMessageFormats => _channelMessageFormats?.AsEnumerable();
    public IEnumerable<ServerBoosterHistoryEntry>? ServerBoosterHistoryEntries => _serverBoosterHistoryEntries?.AsEnumerable();

    public void AddServerBoosterHistoryEntry(ServerBoosterHistoryEntry entry)
    {
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(entry);
    }
    
    public void AddServerBoosterHistoryEntry(ulong userId, string username, long memberEntryId, DateTime? dateOverride = null)
    {
        var date = dateOverride ?? DateTime.UtcNow;
        _serverBoosterHistoryEntries ??= new HashSet<ServerBoosterHistoryEntry>();
        _serverBoosterHistoryEntries.Add(new ServerBoosterHistoryEntry { UserId = userId, GuildId = this.GuildId, CreatedAt = date, Username = username } );
    }

    public void DisableServerBoosterHistoryEntry(ServerBoosterHistoryEntry entry)
        => DisableServerBoosterHistoryEntry(entry.UserId);
    
    public void DisableServerBoosterHistoryEntry(ulong userId)
    {
        if (_serverBoosterHistoryEntries is null)
            throw new DataException("Data wasn't loaded from the database.");

        var current =
            _serverBoosterHistoryEntries.Where(x => x.UserId == userId && !x.IsDisabled && x.GuildId == GuildId);

        foreach (var curr in current)
            curr.IsDisabled = true;
    }
    
    public void AddMemberHistoryEntry(MemberHistoryEntry entry)
    {
        _memberHistoryEntries ??= new HashSet<MemberHistoryEntry>();
        _memberHistoryEntries.Add(entry);
    }
    
    public void AddMemberHistoryEntry(ulong userId, string username, DateTime accountCreated, DateTime? dateOverride = null)
    {
        var date = dateOverride ?? DateTime.UtcNow;
        _memberHistoryEntries ??= new HashSet<MemberHistoryEntry>();
        _memberHistoryEntries.Add(new MemberHistoryEntry { UserId = userId, GuildId = this.GuildId, CreatedAt = date, Username = username, AccountCreated = accountCreated } );
    }

    public void DisableMemberHistoryEntry(MemberHistoryEntry entry)
        => DisableServerBoosterHistoryEntry(entry.UserId);
    
    public void DisableMemberHistoryEntry(ulong userId)
    {
        if (_memberHistoryEntries is null)
            throw new DataException("Data wasn't loaded from the database.");

        var current =
            _memberHistoryEntries.Where(x => x.UserId == userId && !x.IsDisabled && x.GuildId == GuildId);

        foreach (var curr in current)
            curr.IsDisabled = true;
    }
    
    public void AddMute(Mute mute)
    {
        if (mute is null) throw new ArgumentNullException(nameof(mute));
        _mutes?.Add(mute);
    }

    public void AddPrune(Prune prune)
    {
        if (prune is null) throw new ArgumentNullException(nameof(prune));
        _prunes?.Add(prune);
    }

    public void AddBan(Ban ban)
    {
        if (ban is null) throw new ArgumentNullException(nameof(ban));
        _bans?.Add(ban);
    }

    public bool AddTag(Tag tag)
    {
        if (tag is null) throw new ArgumentNullException(nameof(tag));
        return _tags is not null && _tags.Add(tag);
    }

    public bool RemoveTag(string name)
    {
        if (name is "") throw new ArgumentException("Name can't be empty", nameof(name));

        var tag = Tags?.FirstOrDefault(x => x.Name == name);

        if (tag is null) return false;

        tag.IsDisabled = true;

        return true;
    }

    public bool ReplaceTag(Tag tag)
    {
        if (tag is null) throw new ArgumentNullException(nameof(tag));
        var res = _tags?.RemoveWhere(x => x.Name == tag.Name);
        return _tags is not null && res != 0 && _tags.Add(tag);
    }

    public bool AddChannelMessageFormat(ChannelMessageFormat format)
    {
        if (format is null) throw new ArgumentNullException(nameof(format));
        //format.Guild = this;
        return _channelMessageFormats is not null && _channelMessageFormats.Add(format);
    }

    public bool RemoveChannelMessageFormat(ulong channelId)
    {
        var format = ChannelMessageFormats?.FirstOrDefault(x => x.ChannelId == channelId);

        if (format is null) return false;

        format.IsDisabled = true;

        return true;
    }

    public bool ReplaceChannelMessageFormat(ChannelMessageFormat format)
    {
        if (format is null) throw new ArgumentNullException(nameof(format));
        var res = _channelMessageFormats?.RemoveWhere(x => x.ChannelId == format.ChannelId);
        return _channelMessageFormats is not null && res != 0 && _channelMessageFormats.Add(format);
    }

    public void SetTicketingConfig(TicketingConfig config)
    {
        TicketingConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void SetModerationConfig(ModerationConfig config)
    {
        ModerationConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    [MemberNotNullWhen(true, nameof(TicketingConfig))]
    public bool IsTicketingModuleEnabled
        => TicketingConfig is not null;

    [MemberNotNullWhen(true, nameof(ModerationConfig))]
    public bool IsModerationModuleEnabled
        => ModerationConfig is not null;


    [MemberNotNullWhen(true, nameof(ModerationConfig))]
    public bool IsReminderModuleEnabled
        => ReminderChannelId is not null;

    public bool IsDisabled { get; set; }
}
