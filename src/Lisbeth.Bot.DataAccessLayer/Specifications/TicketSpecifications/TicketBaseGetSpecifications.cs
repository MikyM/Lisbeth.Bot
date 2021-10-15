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

using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications
{
    public class TicketBaseGetSpecifications : Specifications<Ticket>
    {
        public TicketBaseGetSpecifications(long? id = null, ulong? userId = null, ulong? guildId = null,
            ulong? channelId = null, long? guildSpecificId = null, bool isDisabled = false, int limit = 0)
        {
            if (id is not null)
                AddFilterCondition(x => x.Id == id);
            if (userId is not null)
                AddFilterCondition(x => x.UserId == userId);
            if (guildId is not null)
                AddFilterCondition(x => x.GuildId == guildId);
            if (channelId is not null)
                AddFilterCondition(x => x.ChannelId == channelId);
            if (guildSpecificId is not null)
                AddFilterCondition(x => x.GuildSpecificId == guildSpecificId);

            AddFilterCondition(x => x.IsDisabled == isDisabled);

            ApplyOrderByDescending(x => x.CreatedAt);

            ApplyLimit(limit);
        }
    }
}