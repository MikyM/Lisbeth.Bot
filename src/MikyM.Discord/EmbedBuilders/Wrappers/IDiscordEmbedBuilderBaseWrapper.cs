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
using System;
using System.Collections.Generic;

namespace MikyM.Discord.EmbedBuilders.Wrappers;

/// <summary>
/// An immutable wrapper for <see cref="DiscordEmbedBuilder"/>.
/// </summary>
public interface IDiscordEmbedBuilderBaseWrapper
{
    /// <summary>
    /// Gets the embed's title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the embed's description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the url for the embed's title.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the embed's color.
    /// </summary>
    Optional<DiscordColor> Color { get; }

    /// <summary>
    /// Gets the embed's timestamp.
    /// </summary>
    DateTimeOffset? Timestamp { get; }

    /// <summary>
    /// Gets the embed's image url.
    /// </summary>
    string ImageUrl { get; }

    /// <summary>
    /// Gets the embed's author.
    /// </summary>
    DiscordEmbedBuilder.EmbedAuthor Author { get; }

    /// <summary>
    /// Gets the embed's footer.
    /// </summary>
    DiscordEmbedBuilder.EmbedFooter Footer { get; }

    /// <summary>
    /// Gets the embed's thumbnail.
    /// </summary>
    DiscordEmbedBuilder.EmbedThumbnail Thumbnail { get; }

    /// <summary>
    /// Gets the embed's fields.
    /// </summary>
    IReadOnlyList<DiscordEmbedField> Fields { get; }
}