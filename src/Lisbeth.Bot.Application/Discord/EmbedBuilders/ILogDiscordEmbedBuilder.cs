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

using MikyM.Discord.EmbedBuilders.Builders;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedBuilders;

public interface ILogDiscordEmbedBuilder : IEnrichedDiscordEmbedBuilder
{
    public DiscordLog? Log { get; }
    public DiscordModeration? Moderation { get; }

    ILogDiscordEmbedBuilder WithType(DiscordLog log);
    ILogDiscordEmbedBuilder WithModerationType(DiscordModeration moderation);
    new ILogDiscordEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher) where TEnricher : IEmbedEnricher;
}
