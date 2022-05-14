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

using System.Linq.Expressions;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Common.EfCore.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.EmbedConfig;

public class ActiveSnowflakeWithGivenEmbedSpec<T, TEmbedProperty> : Specification<T> where T : SnowflakeDiscordEntity
    where TEmbedProperty : Domain.Entities.EmbedConfig?
{
    public ActiveSnowflakeWithGivenEmbedSpec(Expression<Func<T, TEmbedProperty?>> embedToInclude, ulong guildId)
    {
        Where(x => x.GuildId == guildId);
        Include(embedToInclude);
    }
}

public class ActiveEmbedEntityWithGivenEmbedSpec<T, TEmbedProperty> : ActiveSnowflakeWithGivenEmbedSpec<T, TEmbedProperty> where T : EmbedConfigEntity
    where TEmbedProperty : Domain.Entities.EmbedConfig
{
    public ActiveEmbedEntityWithGivenEmbedSpec(Expression<Func<T, TEmbedProperty?>> embedToInclude, ulong guildId, string name) : base(embedToInclude, guildId)
    {
        Where(x => x.Name == name);
    }
}
