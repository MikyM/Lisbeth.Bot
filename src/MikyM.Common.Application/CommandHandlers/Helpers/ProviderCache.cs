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

namespace MikyM.Common.Application.CommandHandlers.Helpers;

internal static class ProviderCache
{
    static ProviderCache()
    {
        CachedTypes ??= AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes()
                .Where(t => t.GetInterfaces().Any(y => y == typeof(ICommandHandler)) && !t.IsAbstract && t.IsClass &&
                            !t.IsGenericType))
            .ToDictionary(x =>
                x.GetInterfaces().FirstOrDefault(y => y.IsGenericType)?.GenericTypeArguments ??
                throw new InvalidOperationException("Found an invalid command handler"));
    }

    internal static Dictionary<Type[], Type> CachedTypes { get; }
}