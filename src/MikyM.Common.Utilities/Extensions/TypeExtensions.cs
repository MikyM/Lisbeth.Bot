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

using MikyM.Common.Utilities.Optionals;

namespace MikyM.Common.Utilities.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited, bool fullSet = true)
    {
        if ((includeInherited || type.BaseType is null) && fullSet)
            return type.GetInterfaces();

        var allInterfaces = type.GetInterfaces();

        switch (includeInherited)
        {
            case false when !fullSet:
                return type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces()));
            case true when type.BaseType is not null && !fullSet:
            {
                var res = type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces())).ToList();
                res.AddRange(type.BaseType.GetInterfaces());

                return res;
            }
            default:
                return type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces())).ToList();
        }
    }

    /// <summary>
    /// Determines whether the given type is a closed <see cref="Optional{TValue}"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is a closed Optional; otherwise, false.</returns>
    public static bool IsOptional(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    /// <summary>
    /// Determines whether the given type is a closed <see cref="Nullable{TValue}"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is a closed Nullable; otherwise, false.</returns>
    public static bool IsNullable(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Retrieves the innermost type from a type wrapped by
    /// <see cref="Nullable{T}"/> or <see cref="Optional{TValue}"/>.
    /// </summary>
    /// <param name="type">The type to unwrap.</param>
    /// <returns>The unwrapped type.</returns>
    public static Type Unwrap(this Type type)
    {
        var currentType = type;
        while (currentType.IsGenericType)
        {
            if (currentType.IsOptional() || currentType.IsNullable())
            {
                currentType = currentType.GetGenericArguments()[0];
                continue;
            }

            break;
        }

        return currentType;
    }

    /// <summary>
    /// Gets a dictionary with interface implementation pairs that implement a given base interface.
    /// </summary>
    /// <param name="interfaceToSearchFor">Base interface to search for.</param>
    public static Dictionary<Type, Type?> GetInterfaceImplementationPairs(this Type interfaceToSearchFor)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var dict = assemblies
            .SelectMany(x => x.GetTypes()
                .Where(t => interfaceToSearchFor.IsDirectAncestor(t) &&
                            t.IsInterface))
            .ToDictionary(intr => intr,
                intr => assemblies.SelectMany(impl => impl.GetTypes())
                    .FirstOrDefault(impl =>
                        impl.IsAssignableToWithGenerics(intr) && impl.IsClass &&
                        intr.IsDirectAncestor(impl)));

        return dict;
    }

    public static bool IsAssignableToWithGenerics(this Type givenType, Type genericType)
    {
        if (!genericType.IsGenericType)
            return givenType.IsAssignableTo(genericType);

        var interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            return true;
        
        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type? baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToWithGenerics(baseType, genericType);
    }

    /// <summary>
    /// Retrieves the type inheritance tree
    /// </summary>
    /// <param name="type">The type to find tree for.</param>
    /// <returns>The inheritance tree.</returns>
    public static InheritanceTree GetTypeInheritance(this Type type)
    {
        //get all the interfaces for this type
        var interfaces = type.GetInterfaces();

        //get all the interfaces for the ancestor interfaces
        var baseInterfaces = interfaces.SelectMany(i => i.GetInterfaces());

        //filter based on only the direct interfaces
        var directInterfaces = interfaces.Where(i => baseInterfaces.All(b => b != i));

        return new InheritanceTree(type, directInterfaces.Select(GetTypeInheritance).ToList());
    }

    /// <summary>
    /// Check if a type is a direct ancestor of given type
    /// </summary>
    public static bool IsDirectAncestor(this Type ancestorCandidate, Type type)
        => type.GetTypeInheritance().IsDirectAncestor(ancestorCandidate);

    /// <summary>
    /// Inheritance tree
    /// </summary>
    public class InheritanceTree
    {
        public InheritanceTree(Type node, List<InheritanceTree> ancestors)
        {
            Node = node;
            Ancestors = ancestors;
        }

        public Type Node { get; set; }
        public List<InheritanceTree> Ancestors { get; set; }

        public bool IsDirectAncestor(Type type)
            => Ancestors.Any(x =>
                (x.Node.IsGenericType ? x.Node.GetGenericTypeDefinition() : x.Node) ==
                (type.IsGenericType ? type.GetGenericTypeDefinition() : type));
    }
}