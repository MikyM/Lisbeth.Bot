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

namespace MikyM.Autofac.Extensions.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FindConstructorsWithAttribute : Attribute
{
    public Type ConstructorFinder { get; set; }
    public FindConstructorsWithAttribute(Type constructorFinder)
    {
        if (constructorFinder is null) throw new ArgumentNullException(nameof(constructorFinder));
        if (!constructorFinder.IsAssignableTo(typeof(IConstructorFinder)))
            throw new InvalidOperationException("Invalid constructor finder type");
        ConstructorFinder = constructorFinder;
    }
}