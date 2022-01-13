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

using Autofac.Core.Activators.Reflection;
using System.Collections.Concurrent;
using System.Reflection;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Constructor finder that finds all constructors
/// </summary>
public sealed class AllConstructorsFinder : IConstructorFinder
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo[]> Cache = new();


    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        var result = Cache.GetOrAdd(targetType,
            t => t.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic).ToArray());

        return result.Length > 0 ? result : throw new NoConstructorsFoundException(targetType);
    }
}
