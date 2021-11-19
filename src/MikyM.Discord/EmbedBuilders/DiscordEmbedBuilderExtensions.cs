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
using MikyM.Discord.EmbedBuilders.Builders;
using System;
using System.Linq;

namespace MikyM.Discord.EmbedBuilders;

public static class DiscordEmbedBuilderExtensions
{
    public static bool IsValid(this DiscordEmbedBuilder builder)
    {
        return !(builder.Author?.Name.Length + builder.Footer?.Text.Length + builder.Description?.Length +
            builder.Title?.Length + builder.Fields?.Sum(x => x.Value.Length + x.Name.Length) > 6000);
    }

    public static IEnhancedDiscordEmbedBuilder Enhance(this DiscordEmbedBuilder builder)
    {
        return new EnhancedDiscordEmbedBuilder(builder);
    }

    public static TBuilder As<TBuilder>(this IEnhancedDiscordEmbedBuilder builder, params object[]? args)
        where TBuilder : IEnrichedEmbedBuilder
    {
        if (!BuilderCache.CachedTypes.TryGetValue(typeof(TBuilder).FullName ?? throw new InvalidOperationException(), out var concrete) || !typeof(TBuilder).IsAssignableFrom(concrete))
            throw new ArgumentException("Given builder type is not valid in this context.");

        return (TBuilder)Activator.CreateInstance(concrete ?? throw new InvalidOperationException(),
            args is null 
                ? builder 
                : args.Concat(new object[] { builder }).ToArray())! 
               ?? throw new InvalidOperationException();
    }
}