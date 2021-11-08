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

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming

namespace Lisbeth.Bot.Domain.Entities
{
    public sealed class Guild : SnowflakeEntity
    {
        private readonly HashSet<Ban>? bans;
        private readonly HashSet<GuildServerBooster>? guildServerBoosters;
        private readonly HashSet<Mute>? mutes;
        private readonly HashSet<Prune>? prunes;
        private readonly HashSet<RecurringReminder>? recurringReminders;
        private readonly HashSet<Reminder>? reminders;
        private readonly HashSet<Ticket>? tickets;
        private readonly HashSet<Tag>? tags;
        private readonly HashSet<RoleMenu>? roleMenus;

        public Guild()
        {
            this.bans ??= new HashSet<Ban>();
            this.guildServerBoosters ??= new HashSet<GuildServerBooster>();
            this.mutes ??= new HashSet<Mute>();
            this.prunes ??= new HashSet<Prune>();
            this.recurringReminders ??= new HashSet<RecurringReminder>();
            this.reminders ??= new HashSet<Reminder>();
            this.tickets ??= new HashSet<Ticket>();
            this.tags ??= new HashSet<Tag>();
            this.roleMenus ??= new HashSet<RoleMenu>();
        }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong? ReminderChannelId { get; set; }
        public TicketingConfig? TicketingConfig { get; private set; }
        public ModerationConfig? ModerationConfig { get; private set; }
        public string EmbedHexColor { get; set; } = "#26296e";
        public IReadOnlyCollection<Mute>? Mutes => mutes;
        public IReadOnlyCollection<Ban>? Bans => bans;
        public IReadOnlyCollection<Prune>? Prunes => prunes;
        public IReadOnlyCollection<Ticket>? Tickets => tickets;
        public IReadOnlyCollection<GuildServerBooster>? GuildServerBoosters => guildServerBoosters;
        public IReadOnlyCollection<Reminder>? Reminders => reminders;
        public IReadOnlyCollection<RecurringReminder>? RecurringReminders => recurringReminders;
        public IReadOnlyCollection<Tag>? Tags => tags;
        public IReadOnlyCollection<RoleMenu>? RoleMenus => roleMenus;

/*        public Guild()
        {
            tags ??= new HashSet<Tag>();
        }*/

        public void AddMute(Mute mute)
        {
            if (mute  is null) throw new ArgumentNullException(nameof(mute));
            mutes?.Add(mute);
        }

        public void AddPrune(Prune prune)
        {
            if (prune  is null) throw new ArgumentNullException(nameof(prune));
            prunes?.Add(prune);
        }

        public void AddBan(Ban ban)
        {
            if (ban  is null) throw new ArgumentNullException(nameof(ban));
            bans?.Add(ban);
        }

        public void AddServerBooster(GuildServerBooster guildServerBooster)
        {
            if (guildServerBooster  is null) throw new ArgumentNullException(nameof(guildServerBooster));
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
            var res = tags?.RemoveWhere(x => x.Name == name);
            return res > 0;
        }

        public bool EditTag(Tag tag)
        {
            if (tag  is null) throw new ArgumentNullException(nameof(tag));
            var res = tags?.RemoveWhere(x => x.Name == tag.Name);
            return tags is not null && res != 0 && tags.Add(tag);
        }

        public void SetTicketingConfig(TicketingConfig config)
        {
            TicketingConfig = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void SetModerationConfig(ModerationConfig config)
        {
            ModerationConfig = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}