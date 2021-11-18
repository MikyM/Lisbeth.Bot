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
public abstract class EnrichedEmbedBuilder<TEnhancement> : IEnrichedEmbedBuilder<TEnhancement> where TEnhancement : Enum
{
    /// <summary>
    /// Gets the previous builder that was used to construct this one.
    /// </summary>
    protected EnhancedDiscordEmbedBuilder<TEnhancement> BaseBuilder { get; private set; }

    private DiscordEmbedBuilder _current;
    /// <summary>
    /// Gets the current embed builder.
    /// </summary>
    protected DiscordEmbedBuilder Current
    {
        get
        {
            this.Evaluate();
            return _current;
        }
        private set => this._current = value;
    }
    /// <summary>
    /// Gets the base embed builder that was supplied by the previous builder.
    /// </summary>
    protected DiscordEmbedBuilder Base { get; private set; }

    /// <summary>
    /// Constructs an enriched embed builder.
    /// </summary>
    /// <param name="baseEmbedBuilder">Builder to base this off of.</param>
    /// <param name="action">Specific name of the action that the embed responds to if any.</param>
    protected EnrichedEmbedBuilder(EnhancedDiscordEmbedBuilder<TEnhancement> baseEmbedBuilder)
    {
        this.Base = new DiscordEmbedBuilder(baseEmbedBuilder.Current);
        this._current = new DiscordEmbedBuilder(this.Base);
        this.BaseBuilder = baseEmbedBuilder;
    }

    public abstract IEnrichedEmbedBuilder<TEnhancement> EnrichFrom<TEnricher>(TEnricher enricher)
        where TEnricher : IEmbedEnricher;

    /// <summary>
    /// Prepares the builder for building.
    /// </summary>
    protected abstract void Evaluate();

    public DiscordEmbed Build()
        => this.Current.Build();

    public static implicit operator DiscordEmbed(EnrichedEmbedBuilder<TEnhancement> builder)
        => builder.Build();

    /// <summary>
    /// Sets the action type.
    /// </summary>
    protected IEnrichedEmbedBuilder<TEnhancement> WithEnhancementAction<TEnum>(TEnum action) where TEnum : Enum
    {
        this.BaseBuilder.WithEnhancementAction(action);
        this.Current = new DiscordEmbedBuilder(this.BaseBuilder.Current);
        this.Evaluate();
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithDescription(string description)
    {
        this.Current.WithDescription(description);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithUrl(string url)
    {
        this.Current.WithUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithUrl(Uri url)
    {
        this.Current.WithUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithColor(DiscordColor color)
    {
        this.Current.WithColor(color);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithTimestamp(DateTimeOffset? timestamp)
    {
        this.Current.WithTimestamp(timestamp);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithTimestamp(DateTime? timestamp)
    {
        this.Current.WithTimestamp(timestamp);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithTimestamp(ulong snowflake)
    {
        this.Current.WithTimestamp(snowflake);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithImageUrl(string url)
    {
        this.Current.WithImageUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithImageUrl(Uri url)
    {
        this.Current.WithImageUrl(url);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithThumbnail(string url, int height = 0, int width = 0)
    {
        this.Current.WithThumbnail(url, height, width);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithThumbnail(Uri url, int height = 0, int width = 0)
    {
        this.Current.WithThumbnail(url, height, width);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithAuthor(string? name = null, string? url = null, string? iconUrl = null)
    {
        this.Current.WithAuthor(name, url, iconUrl);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> WithFooter(string? text = null, string? iconUrl = null)
    {
        this.Current.WithFooter(text, iconUrl);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> AddField(string name, string value, bool inline = false)
    {
        this.Current.AddField(name, value, inline);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> RemoveFieldAt(int index)
    {
        this.Current.RemoveFieldAt(index);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> RemoveFieldRange(int index, int count)
    {
        this.Current.RemoveFieldRange(index, count);
        return this;
    }

    public IEnrichedEmbedBuilder<TEnhancement> ClearFields()
    {
        this.Current.ClearFields();
        return this;
    }
}
