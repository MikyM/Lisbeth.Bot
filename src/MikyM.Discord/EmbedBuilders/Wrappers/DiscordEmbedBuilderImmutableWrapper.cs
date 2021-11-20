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
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace MikyM.Discord.EmbedBuilders.Wrappers;

/// <summary>
/// An immutable wrapper for <see cref="DiscordEmbedBuilder"/>.
/// </summary>
public class DiscordEmbedBuilderImmutableWrapper : IDiscordEmbedBuilderBaseWrapper
{
    /// <summary>
    /// Gets the current embed builder.
    /// </summary>
    protected DiscordEmbedBuilder Wrapped { get; }

    public string Title => this.Wrapped.Title;

    public string Description => this.Wrapped.Description;

    public string Url => this.Wrapped.Url;

    public Optional<DiscordColor> Color => this.Wrapped.Color;

    public DateTimeOffset? Timestamp => this.Wrapped.Timestamp;

    public string ImageUrl => this.Wrapped.ImageUrl;

    public DiscordEmbedBuilder.EmbedAuthor Author => this.Wrapped.Author;

    public DiscordEmbedBuilder.EmbedFooter Footer => this.Wrapped.Footer;

    public DiscordEmbedBuilder.EmbedThumbnail Thumbnail => this.Wrapped.Thumbnail;

    public IReadOnlyList<DiscordEmbedField> Fields => this.Wrapped.Fields;

    /// <summary>
    /// Wraps an embed builder.
    /// </summary>
    /// <param name="wrapped">Builder to wrap.</param>
    public DiscordEmbedBuilderImmutableWrapper(DiscordEmbedBuilder wrapped)
        => this.Wrapped = new DiscordEmbedBuilder(wrapped ?? throw new ArgumentNullException(nameof(wrapped)));

    /// <summary>
    /// Extracts the builder.
    /// </summary>
    /// <returns>The instance of the currently wrapped builder.</returns>
    internal DiscordEmbedBuilder GetBaseInternal() => this.Wrapped;
}