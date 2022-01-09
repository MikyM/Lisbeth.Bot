// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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

using Lisbeth.Bot.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming

namespace Lisbeth.Bot.Domain.Entities;

public sealed class Guild : SnowflakeEntity
{
    private readonly HashSet<Ban>? bans;
    private readonly HashSet<GuildServerBooster>? guildServerBoosters;
    private readonly HashSet<Mute>? mutes;
    private readonly HashSet<Prune>? prunes;
    private readonly HashSet<Reminder>? reminders;
    private readonly HashSet<RoleMenu>? roleMenus;
    private readonly HashSet<Tag>? tags;
    private readonly HashSet<Ticket>? tickets;
    private readonly HashSet<ChannelMessageFormat>? channelMessageFormats;

    public Guild()
    {
        bans ??= new HashSet<Ban>();
        guildServerBoosters ??= new HashSet<GuildServerBooster>();
        mutes ??= new HashSet<Mute>();
        prunes ??= new HashSet<Prune>();
        reminders ??= new HashSet<Reminder>();
        tickets ??= new HashSet<Ticket>();
        tags ??= new HashSet<Tag>();
        roleMenus ??= new HashSet<RoleMenu>();
        channelMessageFormats ??= new HashSet<ChannelMessageFormat>();
    }

    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong? ReminderChannelId { get; set; }
    public TicketingConfig? TicketingConfig { get; private set; }
    public ModerationConfig? ModerationConfig { get; private set; }
    public string EmbedHexColor { get; set; } = "#26296e";
    public IEnumerable<Mute>? Mutes => mutes?.AsEnumerable();
    public IEnumerable<Ban>? Bans => bans?.AsEnumerable();
    public IEnumerable<Prune>? Prunes => prunes?.AsEnumerable();
    public IEnumerable<Ticket>? Tickets => tickets?.AsEnumerable();
    public IEnumerable<GuildServerBooster>? GuildServerBoosters => guildServerBoosters?.AsEnumerable();
    public IEnumerable<Reminder>? Reminders => reminders?.AsEnumerable();
    public IEnumerable<Tag>? Tags => tags?.AsEnumerable();
    public IEnumerable<RoleMenu>? RoleMenus => roleMenus?.AsEnumerable();
    public IEnumerable<ChannelMessageFormat>? ChannelMessageFormats => channelMessageFormats?.AsEnumerable();

    public void AddMute(Mute mute)
    {
        if (mute is null) throw new ArgumentNullException(nameof(mute));
        mutes?.Add(mute);
    }

    public void AddPrune(Prune prune)
    {
        if (prune is null) throw new ArgumentNullException(nameof(prune));
        prunes?.Add(prune);
    }

    public void AddBan(Ban ban)
    {
        if (ban is null) throw new ArgumentNullException(nameof(ban));
        bans?.Add(ban);
    }

    public void AddServerBooster(GuildServerBooster guildServerBooster)
    {
        if (guildServerBooster is null) throw new ArgumentNullException(nameof(guildServerBooster));
        guildServerBoosters?.Add(guildServerBooster);
    }

    public bool AddTag(Tag tag)
    {
        if (tag is null) throw new ArgumentNullException(nameof(tag));
        return tags is not null && tags.Add(tag);
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
        var res = tags?.RemoveWhere(x => x.Name == tag.Name);
        return tags is not null && res != 0 && tags.Add(tag);
    }

    public bool AddChannelMessageFormat(ChannelMessageFormat format)
    {
        if (format is null) throw new ArgumentNullException(nameof(format));
        //format.Guild = this;
        return channelMessageFormats is not null && channelMessageFormats.Add(format);
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
        var res = channelMessageFormats?.RemoveWhere(x => x.ChannelId == format.ChannelId);
        return channelMessageFormats is not null && res != 0 && channelMessageFormats.Add(format);
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
        => this.TicketingConfig is not null;

    [MemberNotNullWhen(true, nameof(ModerationConfig))]
    public bool IsModerationModuleEnabled
        => this.ModerationConfig is not null;


    [MemberNotNullWhen(true, nameof(ModerationConfig))]
    public bool IsReminderModuleEnabled
        => this.ReminderChannelId is not null;
}