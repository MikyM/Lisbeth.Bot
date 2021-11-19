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

using System;
using DSharpPlus.Entities;
using MikyM.Discord.EmbedBuilders.Enrichers;

namespace MikyM.Discord.EmbedBuilders.Builders;

/// <summary>
/// Constructs enriched embeds.
/// </summary>
public abstract class EnrichedEmbedBuilder : IEnrichedEmbedBuilder
{
    /// <summary>
    /// Gets the previous builder that was used to construct this one.
    /// </summary>
    protected EnhancedDiscordEmbedBuilder EnhancedBuilder { get; private set; }

    /// <summary>
    /// Gets the current embed builder.
    /// </summary>
    protected DiscordEmbedBuilder Current { get; private set; }
    /// <summary>
    /// Gets the base embed builder that was supplied by the previous builder.
    /// </summary>
    protected DiscordEmbedBuilder Base { get; private set; }

    /// <summary>
    /// Constructs an enriched embed builder.
    /// </summary>
    /// <param name="enhancedEmbedBuilder">Builder to base this off of.</param>
    protected EnrichedEmbedBuilder(EnhancedDiscordEmbedBuilder enhancedEmbedBuilder)
    {
        this.Base = new DiscordEmbedBuilder(enhancedEmbedBuilder.Current);
        this.Current = enhancedEmbedBuilder.Current;
        this.EnhancedBuilder = enhancedEmbedBuilder;
    }

    public abstract IEnrichedEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher)
        where TEnricher : IEmbedEnricher;

    /// <summary>
    /// Prepares the builder for building.
    /// </summary>
    protected virtual void Evaluate()
        => this.EnhancedBuilder.Evaluate();
    public DiscordEmbed Build()
        => this.Current.Build();

    public static implicit operator DiscordEmbed(EnrichedEmbedBuilder builder)
        => builder.Build();

    /// <summary>
    /// Sets the action type.
    /// </summary>
    protected IEnrichedEmbedBuilder WithAction<TEnum>(TEnum action) where TEnum : Enum
    {
        this.EnhancedBuilder.WithAction(action);
        this.Evaluate();
        return this;
    }


    /// <summary>
    /// Sets the action.
    /// </summary>
    protected IEnrichedEmbedBuilder WithActionType<TEnum>(TEnum action) where TEnum : Enum
    {
        this.EnhancedBuilder.WithActionType(action);
        this.Current = new DiscordEmbedBuilder(this.EnhancedBuilder.Current);
        this.Evaluate();
        return this;
    }

    public IEnrichedEmbedBuilder WithDescription(string description)
    {
        this.Current.WithDescription(description);
        return this;
    }

    public IEnrichedEmbedBuilder WithUrl(string url)
    {
        this.Current.WithUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder WithUrl(Uri url)
    {
        this.Current.WithUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder WithColor(DiscordColor color)
    {
        this.Current.WithColor(color);
        return this;
    }

    public IEnrichedEmbedBuilder WithTimestamp(DateTimeOffset? timestamp)
    {
        this.Current.WithTimestamp(timestamp);
        return this;
    }

    public IEnrichedEmbedBuilder WithTimestamp(DateTime? timestamp)
    {
        this.Current.WithTimestamp(timestamp);
        return this;
    }

    public IEnrichedEmbedBuilder WithTimestamp(ulong snowflake)
    {
        this.Current.WithTimestamp(snowflake);
        return this;
    }

    public IEnrichedEmbedBuilder WithImageUrl(string url)
    {
        this.Current.WithImageUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder WithImageUrl(Uri url)
    {
        this.Current.WithImageUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder WithThumbnail(string url, int height = 0, int width = 0)
    {
        this.Current.WithThumbnail(url, height, width);
        return this;
    }

    public IEnrichedEmbedBuilder WithThumbnail(Uri url, int height = 0, int width = 0)
    {
        this.Current.WithThumbnail(url, height, width);
        return this;
    }

    public IEnrichedEmbedBuilder WithAuthor(string? name = null, string? url = null, string? iconUrl = null)
    {
        this.Current.WithAuthor(name, url, iconUrl);
        return this;
    }

    public IEnrichedEmbedBuilder WithFooter(string? text = null, string? iconUrl = null)
    {
        this.Current.WithFooter(text, iconUrl);
        return this;
    }

    public IEnrichedEmbedBuilder AddField(string name, string value, bool inline = false)
    {
        this.Current.AddField(name, value, inline);
        return this;
    }

    public IEnrichedEmbedBuilder RemoveFieldAt(int index)
    {
        this.Current.RemoveFieldAt(index);
        return this;
    }

    public IEnrichedEmbedBuilder RemoveFieldRange(int index, int count)
    {
        this.Current.RemoveFieldRange(index, count);
        return this;
    }

    public IEnrichedEmbedBuilder ClearFields()
    {
        this.Current.ClearFields();
        return this;
    }
}
