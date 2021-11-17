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

using DSharpPlus.Entities;
using MikyM.Discord.EmbedBuilders.Enums;

namespace MikyM.Discord.EmbedBuilders.Builders;

public class ResponseEmbedBuilder : EnrichedEmbedBuilder<IResponseEmbedBuilder>, IResponseEmbedBuilder
{
    public virtual DiscordResponse Response { get; private set; }

    protected internal ResponseEmbedBuilder(IEnhancedDiscordEmbedBuilder previousEmbedBuilder) : base(previousEmbedBuilder)
    {
    }

    public IResponseEmbedBuilder WithType(DiscordResponse response)
    {
        this.Response = response;
        return this;
    }

    public override DiscordEmbed Build()
        => this.BaseBuild().Build();

    public override DiscordEmbedBuilder BaseBuild()
    {
        if (this.Response is not 0) // if not default
            this.Base = this.PreviousBuilder.WithEnhancementAction(this.Response.ToString()).BaseBuild();
        return this.Base;
    }

    public override ResponseEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher)
    {
        enricher.Enrich(this);
        return this;
    }
}
