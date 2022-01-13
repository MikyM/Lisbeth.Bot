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

namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines with which lifetime should the service be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LifetimeAttribute : Attribute
{
    public Lifetime Scope { get; private set; }
    public Type? Owned { get; private set; }
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    public LifetimeAttribute(Lifetime scope)
    {
        Scope = scope;
    }

    public LifetimeAttribute(Lifetime scope, Type owned)
    {
        Scope = scope;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public LifetimeAttribute(Lifetime scope, IEnumerable<object> tags)
    {
        Scope = scope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }

    public LifetimeAttribute(Type owned)
    {
        Scope = Lifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public LifetimeAttribute(IEnumerable<object> tags)
    {
        Scope = Lifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }
}