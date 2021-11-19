// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using MikyM.Discord.EmbedBuilders.Enrichers;
using System;

namespace MikyM.Discord.EmbedBuilders.Builders;

/// <summary>
/// Constructs enriched embeds.
/// </summary>
public abstract class EnrichedEmbedBuilder : IEnrichedEmbedBuilder
{
    /// <summary>
    /// Gets the previous builder that was used to construct this one.
    /// </summary>
    protected EnhancedDiscordEmbedBuilder EnhancedBuilder { get; }

    /// <summary>
    /// Gets the discord embed builder wrapper.
    /// </summary>
    protected DiscordEmbedBuilderWrapper BaseWrapper { get; }

    /// <summary>
    /// Gets the current embed builder.
    /// </summary>
    protected DiscordEmbedBuilder Current => this.EnhancedBuilder.Current;

    /// <summary>
    /// Gets the base embed builder that was supplied by the previous builder.
    /// </summary>
    protected DiscordEmbedBuilder Base => this.EnhancedBuilder.Base;

    /// <summary>
    /// Constructs an enriched embed builder.
    /// </summary>
    /// <param name="enhancedEmbedBuilder">Builder to base this off of.</param>
    protected EnrichedEmbedBuilder(EnhancedDiscordEmbedBuilder enhancedEmbedBuilder)
    {
        this.EnhancedBuilder = enhancedEmbedBuilder;
        this.BaseWrapper = new DiscordEmbedBuilderWrapper(this.Current);
    }

    public virtual IEnrichedEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher)
        where TEnricher : IEmbedEnricher
    {
        enricher.Enrich(this.BaseWrapper);
        return this;
    }

    /// <summary>
    /// Prepares the builder for building.
    /// </summary>
    protected virtual void Evaluate()
        => this.EnhancedBuilder.Evaluate();

    public DiscordEmbed Build()
    {
        this.Evaluate();
        return this.Current.Build();
    }

    public static implicit operator DiscordEmbed(EnrichedEmbedBuilder builder)
        => builder.Build();

    public IEnrichedEmbedBuilder WithAction<TEnum>(TEnum action) where TEnum : Enum
    {
        this.EnhancedBuilder.WithAction(action);
        this.Evaluate();
        return this;
    }

    public IEnrichedEmbedBuilder WithActionType<TEnum>(TEnum action) where TEnum : Enum
    {
        this.EnhancedBuilder.WithActionType(action);
        this.Evaluate();
        return this;
    }
    
    public IEnrichedEmbedBuilder WithCase(long caseId)
    {
        this.EnhancedBuilder.WithCase(caseId);
        return this;
    }

    public IEnrichedEmbedBuilder WithFooterSnowflakeInfo(SnowflakeObject snowflake)
    {
        this.EnhancedBuilder.WithFooterSnowflakeInfo(snowflake);
        return this;
    }

    public IEnrichedEmbedBuilder WithAuthorSnowflakeInfo(DiscordMember member)
    {
        this.EnhancedBuilder.WithAuthorSnowflakeInfo(member);
        return this;
    }

    public IEnrichedEmbedBuilder SetAuthorTemplate(string template)
    {
        this.EnhancedBuilder.SetAuthorTemplate(template);
        return this;
    }
    public IEnrichedEmbedBuilder SetFooterTemplate(string template)
    {
        this.EnhancedBuilder.SetFooterTemplate(template);
        return this;
    }

    public IEnrichedEmbedBuilder SetTitleTemplate(string template)
    {
        this.EnhancedBuilder.SetTitleTemplate(template);
        return this;
    }
}
