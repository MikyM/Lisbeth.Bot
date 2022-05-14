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

using MikyM.Common.EfCore.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;

public class ActiveRecurringReminderByNameOrIdAndGuildSpec : Specification<Domain.Entities.Reminder>
{
    public ActiveRecurringReminderByNameOrIdAndGuildSpec(long id) : this("", null, id)
    {
    }

    public ActiveRecurringReminderByNameOrIdAndGuildSpec(string name, ulong guildId) : this(name, guildId, null)
    {
    }

    public ActiveRecurringReminderByNameOrIdAndGuildSpec(string name, ulong? guildId, long? id)
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
        Where(x => x.CronExpression != null);
    }
}