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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MikyM.Discord.EmbedBuilders.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MikyM.Discord.EmbedBuilders;

public static class DependancyInjectionExtensions
{
    /// <summary>
    /// Registers <see cref="IEnhancedDiscordEmbedBuilder"/> with the <see cref="IServiceCollection"/>.
    /// <br></br>It will also try to register other builders implementing <see cref="IEnhancedDiscordEmbedBuilder"/> with their concrete implementations by naming convention.
    /// </summary>
    public static void AddEnhancedDiscordEmbedBuilders(this IServiceCollection services)
    {
        services.TryAddTransient<IEnhancedDiscordEmbedBuilder, EnhancedDiscordEmbedBuilder>();

        var pairs = GetInterfaceImplementationPairsByConvention(typeof(IEnhancedDiscordEmbedBuilder));

        if (pairs.Count == 0) return;

        foreach (var (intr, impl) in pairs)
        {
            if (impl is null) continue;
            services.TryAddTransient(intr, impl);
        }
    }

    /// <summary>
    /// Registers <see cref="IEnrichedDiscordEmbedBuilder"/> with the <see cref="IServiceCollection"/>.
    /// <br></br>It will also try to register other builders implementing <see cref="IEnrichedDiscordEmbedBuilder"/> with their concrete implementations by naming convention.
    /// <br></br>This will also automatically register <see cref="IEnhancedDiscordEmbedBuilder"/>.
    /// </summary>
    public static void AddEnrichedDiscordEmbedBuilders(this IServiceCollection services)
    {
        AddEnhancedDiscordEmbedBuilders(services);

        services.TryAddTransient<IEnrichedDiscordEmbedBuilder, EnrichedDiscordEmbedBuilder>();

        var pairs = GetInterfaceImplementationPairsByConvention(typeof(IEnrichedDiscordEmbedBuilder));

        if (pairs.Count == 0) return;

        foreach (var (intr, impl) in pairs)
        {
            if (impl is null) continue;
            services.TryAddTransient(intr, impl);
        }
    }

    /// <summary>
    /// Gets a dictionary with interface implementation pairs that implement a base interface.
    /// </summary>
    private static Dictionary<Type, Type?> GetInterfaceImplementationPairsByConvention(Type interfaceToSearchFor)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var dict = assemblies
            .SelectMany(x => x.GetTypes()
                .Where(t => t.GetInterfaces().Contains(interfaceToSearchFor) && t.IsInterface))
            .ToDictionary(intr => intr,
                intr => assemblies.SelectMany(impl => impl.GetTypes())
                    .FirstOrDefault(impl => intr.IsAssignableFrom(impl) && impl.Name == intr.Name[1..] && impl.IsClass && !impl.IsGenericType));

        return dict;
    }
}