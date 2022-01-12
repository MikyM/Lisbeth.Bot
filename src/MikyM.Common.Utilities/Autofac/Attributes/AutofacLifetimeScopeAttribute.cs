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

namespace MikyM.Common.Utilities.Autofac.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AutofacLifetimeScopeAttribute : Attribute
{
    public LifetimeScope Scope { get; private set; }
    public Type? Owned { get; private set; }
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    public AutofacLifetimeScopeAttribute(LifetimeScope scope)
    {
        Scope = scope;
    }

    public AutofacLifetimeScopeAttribute(LifetimeScope scope, Type owned)
    {
        Scope = scope;
        Owned = owned;
    }

    public AutofacLifetimeScopeAttribute(LifetimeScope scope, IEnumerable<object> tags)
    {
        Scope = scope;
        Tags = tags;
    }

    public AutofacLifetimeScopeAttribute(Type owned)
    {
        Scope = LifetimeScope.InstancePerOwned;
        Owned = owned;
    }

    public AutofacLifetimeScopeAttribute(IEnumerable<object> tags)
    {
        Scope = LifetimeScope.InstancePerMatchingLifetimeScope;
        Tags = tags;
    }
}