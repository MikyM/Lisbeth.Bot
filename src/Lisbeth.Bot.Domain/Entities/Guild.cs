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

using System;
using System.Collections.Generic;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public sealed class Guild : SnowflakeEntity
    {
        private readonly HashSet<Ban> bans;
        private readonly HashSet<Mute> mutes;
        private readonly HashSet<Prune> prunes;
        private readonly HashSet<Ticket> tickets;

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public TicketingConfig TicketingConfig { get; private set; }
        public ModerationConfig ModerationConfig { get; private set; }
        public string EmbedHexColor { get; set; } = "#26296e";
        public IReadOnlyCollection<Mute> Mutes => mutes;
        public IReadOnlyCollection<Ban> Bans => bans;
        public IReadOnlyCollection<Prune> Prunes => prunes;
        public IReadOnlyCollection<Ticket> Tickets => tickets;

        public void AddMute(Mute mute)
        {
            if (mute is null) throw new ArgumentNullException(nameof(mute));
            mutes.Add(mute);
        }

        public void AddPrune(Prune prune)
        {
            if (prune is null) throw new ArgumentNullException(nameof(prune));
            prunes.Add(prune);
        }

        public void AddBan(Ban ban)
        {
            if (ban is null) throw new ArgumentNullException(nameof(ban));
            bans.Add(ban);
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