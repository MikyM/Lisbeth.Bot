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
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using MikyM.Autofac.Extensions.Attributes;
using System.Reflection;

namespace MikyM.Autofac.Extensions
{
    public static class DependancyInjectionExtensions
    {
        public static ContainerBuilder RegisterServicesByAttributes(this ContainerBuilder builder)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var set = assembly.GetTypes()
                    .Where(x => x.GetCustomAttributes(false)
                        .Any(y => y.GetType() == typeof(ServiceAttribute)) && x.IsClass && !x.IsAbstract)
                    .ToList();

                foreach (var type in set)
                {
                    var intrAttrs = type.GetCustomAttributes<InterceptedByAttribute>(false).ToList();
                    var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                    var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();

                    var scope = scopeAttr?.Scope ?? Lifetime.InstancePerLifetimeScope;

                    var registerAsTypes = asAttrs.Where(x => x.RegisterAsType is not null)
                        .Select(x => x.RegisterAsType)
                        .Distinct()
                        .ToList();
                    var shouldAsSelf = asAttrs.Any(x => x.RegisterAsOption == RegisterAs.Self) &&
                        asAttrs.All(x => x.RegisterAsType != type);
                    var shouldAsInterfaces = !asAttrs.Any() || asAttrs.Any(x => x.RegisterAsOption == RegisterAs.ImplementedInterfaces);

                    var intrEnableAttr = type.GetCustomAttribute<EnableInterceptionAttribute>(false);

                    IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>? registrationGenericBuilder = null;
                    IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>? registrationBuilder = null;

                    if (type.IsGenericType && type.IsGenericTypeDefinition)
                    {
                        if (intrEnableAttr is not null)
                            registrationGenericBuilder = shouldAsInterfaces
                                ? builder.RegisterGeneric(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                                : builder.RegisterGeneric(type).EnableInterfaceInterceptors();
                        else
                            registrationGenericBuilder = shouldAsInterfaces
                                ? builder.RegisterGeneric(type).AsImplementedInterfaces()
                                : builder.RegisterGeneric(type);
                    }
                    else
                    {
                        if (intrEnableAttr is not null)
                        {
                            registrationBuilder = intrEnableAttr.Intercept switch
                            {
                                Intercept.InterfaceAndClass => shouldAsInterfaces
                                    ? builder.RegisterType(type)
                                        .AsImplementedInterfaces()
                                        .EnableClassInterceptors()
                                        .EnableInterfaceInterceptors()
                                    : builder.RegisterType(type)
                                        .EnableClassInterceptors()
                                        .EnableInterfaceInterceptors(),
                                Intercept.Interface => shouldAsInterfaces
                                    ? builder.RegisterType(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                                    : builder.RegisterType(type).EnableInterfaceInterceptors(),
                                Intercept.Class => shouldAsInterfaces
                                    ? builder.RegisterType(type).AsImplementedInterfaces().EnableClassInterceptors()
                                    : builder.RegisterType(type).EnableClassInterceptors(),
                                _ => throw new ArgumentOutOfRangeException()
                            };
                        }
                        else
                        {
                            registrationBuilder = shouldAsInterfaces
                                ? builder.RegisterType(type).AsImplementedInterfaces()
                                : builder.RegisterType(type);
                        }
                    }

                    if (shouldAsSelf)
                    {
                        registrationBuilder = registrationBuilder?.As(type);
                        registrationGenericBuilder = registrationGenericBuilder?.AsSelf();
                    }

                    foreach (var asType in registerAsTypes)
                    {
                        if (asType is null)
                            throw new InvalidOperationException("Type was null during registration");

                        registrationBuilder = registrationBuilder?.As(asType);
                        registrationGenericBuilder = registrationGenericBuilder?.As(asType);
                    }

                    switch (scope)
                    {
                        case Lifetime.Singleton:
                            registrationBuilder = registrationBuilder?.SingleInstance();
                            registrationGenericBuilder = registrationGenericBuilder?.SingleInstance();
                            break;
                        case Lifetime.InstancePerRequest:
                            registrationBuilder = registrationBuilder?.InstancePerRequest();
                            registrationGenericBuilder = registrationGenericBuilder?.InstancePerRequest();
                            break;
                        case Lifetime.InstancePerLifetimeScope:
                            registrationBuilder = registrationBuilder?.InstancePerLifetimeScope();
                            registrationGenericBuilder = registrationGenericBuilder?.InstancePerLifetimeScope();
                            break;
                        case Lifetime.InstancePerDependancy:
                            registrationBuilder = registrationBuilder?.InstancePerDependency();
                            registrationGenericBuilder = registrationGenericBuilder?.InstancePerDependency();
                            break;
                        case Lifetime.InstancePerMatchingLifetimeScope:
                            registrationBuilder =
                                registrationBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags.ToArray() ??
                                    Array.Empty<object>());
                            registrationGenericBuilder =
                                registrationGenericBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags.ToArray() ??
                                    Array.Empty<object>());
                            break;
                        case Lifetime.InstancePerOwned:
                            if (scopeAttr?.Owned is null) throw new InvalidOperationException("Owned type was null");

                            registrationBuilder = registrationBuilder?.InstancePerOwned(scopeAttr.Owned);
                            registrationGenericBuilder = registrationGenericBuilder?.InstancePerOwned(scopeAttr.Owned);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    foreach (var attr in intrAttrs)
                    {
                        registrationBuilder = attr.IsAsync
                            ? registrationBuilder?.InterceptedBy(
                                typeof(AsyncInterceptorAdapter<>).MakeGenericType(attr.Interceptor))
                            : registrationBuilder?.InterceptedBy(attr.Interceptor);
                        registrationGenericBuilder = attr.IsAsync
                            ? registrationGenericBuilder?.InterceptedBy(
                                typeof(AsyncInterceptorAdapter<>).MakeGenericType(attr.Interceptor))
                            : registrationGenericBuilder?.InterceptedBy(attr.Interceptor);
                    }
                }
            }

            return builder;
        }
    }
}
