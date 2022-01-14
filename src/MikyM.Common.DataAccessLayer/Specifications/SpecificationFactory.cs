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

using Autofac;
using System.Collections.Concurrent;

namespace MikyM.Common.DataAccessLayer.Specifications;

public class SpecificationFactory : ISpecificationFactory
{
    private readonly ILifetimeScope _lifetimeScope;
    private ConcurrentDictionary<string, ISpecification>? _specifications;

    public SpecificationFactory(ILifetimeScope scope)
    {
        _lifetimeScope = scope;
    }

    public TSpecification GetSpecification<TSpecification>() where TSpecification : ISpecification
    {
        if (!typeof(TSpecification).IsClass)
            throw new ArgumentException("You can only resolve class types");

        _specifications ??= new ConcurrentDictionary<string, ISpecification>();

        var type = typeof(TSpecification);
        string name = type.FullName ?? throw new InvalidOperationException();

        if (_specifications.TryGetValue(name, out var handler))
            return (TSpecification)handler;

        var other = _specifications.Values.FirstOrDefault(x => x.GetType().IsAssignableTo(type));
        if (other is not null)
            return (TSpecification)other;

        if (_specifications.TryAdd(name, _lifetimeScope.Resolve<TSpecification>()))
            return (TSpecification)_specifications[name];

        if (_specifications.TryGetValue(name, out handler))
            return (TSpecification)handler;

        throw new InvalidOperationException($"Couldn't add nor retrieve handler of type {name}");
    }
}