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

using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.Reminder
{
    public class ActiveReminderByNameOrIdAndGuildSpec : Specification<Domain.Entities.Reminder>
    {
        public ActiveReminderByNameOrIdAndGuildSpec(long id) : this("", null, id) {}
        public ActiveReminderByNameOrIdAndGuildSpec(string name, ulong guildId) : this(name, guildId, null) {}

        public ActiveReminderByNameOrIdAndGuildSpec(string name, ulong? guildId, long? id)
        {
            Where(x => !x.IsDisabled);
            if (id.HasValue)
            {
                Where(x => x.Id == id);
            }
            else
            {
                Where(x => x.GuildId == guildId);
                Where(x => x.Name == name);
            }
        }
    }
}
