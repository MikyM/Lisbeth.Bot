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
using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications
{
    public class ActiveGuildByDiscordIdWithTagsSpecifications : Specifications<Guild>
    {
        public ActiveGuildByDiscordIdWithTagsSpecifications(ulong discordGuildId)
        {
            AddFilterCondition(x => !x.IsDisabled);
            AddFilterCondition(x => x.GuildId == discordGuildId);
            //AddInclude($"{nameof(Guild.Tags)}.{nameof(EmbedConfig)}");
            AddInclude(x => x.Include(y => y.Tags).ThenInclude(z => z.EmbedConfig));
        }
    }
}