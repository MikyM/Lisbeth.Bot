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
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.Services;
using MikyM.Common.Utilities.Autofac;
using MikyM.Common.Utilities.Autofac.Attributes;
using System.Reflection;

namespace MikyM.Common.Application;

public static class DependancyInjectionExtensions
{
    public static void AddApplicationLayer(this ContainerBuilder builder, Action<RegistrationConfiguration> configuration)
    {
        //register async interceptor adapter
        builder.RegisterGeneric(typeof(AsyncInterceptorAdapter<>));
        //register async interceptor
        builder.Register(x => new LoggingInterceptor(x.Resolve<ILoggerFactory>().CreateLogger(nameof(LoggingInterceptor))));

        var config = new RegistrationConfiguration();
        configuration(config);

        var method = typeof(Autofac.RegistrationExtensions).GetMethods().First(x =>
            x.Name == "Register" && x.GetGenericArguments().Count() == 1 &&
            x.GetParameters().Count() == 2);
        MethodInfo? registerMethod;

        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registReadOnlyBuilder;
        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registCrudBuilder;

        switch (config.BaseGenericDataServiceLifetimeScope)
        {
            case LifetimeScope.Singleton:
                registReadOnlyBuilder = builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                    .SingleInstance();
                registCrudBuilder = builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                    .SingleInstance();
                break;
            case LifetimeScope.InstancePerRequest:
                registReadOnlyBuilder = builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                    .InstancePerRequest();
                registCrudBuilder = builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                    .InstancePerRequest();
                break;
            case LifetimeScope.InstancePerLifetimeScope:
                registReadOnlyBuilder = builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                    .InstancePerLifetimeScope();
                registCrudBuilder = builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                    .InstancePerLifetimeScope();
                break;
            case LifetimeScope.InstancePerMatchingLifetimeScope:
                registReadOnlyBuilder = builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                    .InstancePerMatchingLifetimeScope();
                registCrudBuilder = builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                    .InstancePerMatchingLifetimeScope();
                break;
            case LifetimeScope.InstancePerDependancy:
                registReadOnlyBuilder = builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                    .InstancePerDependency();
                registCrudBuilder = builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                    .InstancePerDependency();
                break;
            case LifetimeScope.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(config.BaseGenericDataServiceLifetimeScope), config.BaseGenericDataServiceLifetimeScope, null);
        }

        foreach (var (interceptorType, (action, dataConfig)) in config.DataInterceptorDelegates)
        {
            var act = new DataServiceRegistrationConfiguration();
            dataConfig?.Invoke(act);

            if (!act.ShouldRegisterForReadOnlyServices && !act.ShouldRegisterForCrudServices)
                throw new InvalidOperationException();

            if (act.ShouldRegisterForCrudServices)
                registCrudBuilder.InterceptedBy(interceptorType);

            if (act.ShouldRegisterForReadOnlyServices)
                registReadOnlyBuilder.InterceptedBy(interceptorType);

            registerMethod = method.MakeGenericMethod(interceptorType);
            registerMethod.Invoke(null, new[] { builder, action });
        }

        foreach (var (interceptorType, action) in config.InterceptorDelegates)
        {
            registerMethod = method.MakeGenericMethod(interceptorType);
            registerMethod.Invoke(null, new[] { builder, action });
        }

        var excluded = new[] { typeof(DataServiceBase<>), typeof(CrudService<,>), typeof(ReadOnlyDataService<,>) };

        builder.RegisterAutoMapper(opt => opt.AddExpressionMapping(), false, AppDomain.CurrentDomain.GetAssemblies());

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var subSet = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false)
                    .Any(y => y.GetType() == typeof(AutofacServiceAttribute)) && x.IsClass && !x.IsAbstract)
                .ToList();

            var dataSubSet = assembly.GetTypes()
                .Where(x => x.GetInterfaces()
                    .Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IDataServiceBase<>)) && x.IsClass && !x.IsAbstract)
                .ToList();

            subSet.RemoveAll(x => excluded.Any(y => y == x));
            dataSubSet.RemoveAll(x => excluded.Any(y => y == x));

            foreach (var dataType in dataSubSet)
            {
                var overrideAttr = dataType.GetCustomAttribute<AutofacLifetimeScopeAttribute>();
                var dataIntrAttr = dataType.GetCustomAttributes<InterceptAttribute>(false).ToList();
                bool dataIsIntercepted = dataIntrAttr.Any();
                if (overrideAttr is not null)
                {
                    switch (overrideAttr.Scope)
                    {
                        case LifetimeScope.Singleton:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .SingleInstance();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .SingleInstance();
                            }
                            else
                            {
                                if (dataType.IsGenericType)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .SingleInstance();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .SingleInstance();
                            }
                            break;
                        case LifetimeScope.InstancePerRequest:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerRequest();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerRequest();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerRequest();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerRequest();
                            }
                            break;
                        case LifetimeScope.InstancePerLifetimeScope:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerLifetimeScope();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerLifetimeScope();
                            }
                            break;
                        case LifetimeScope.InstancePerDependancy:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerDependency();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerDependency();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerDependency();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerDependency();
                            }
                            break;
                        case LifetimeScope.InstancePerMatchingLifetimeScope:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerMatchingLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerMatchingLifetimeScope();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerMatchingLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerMatchingLifetimeScope();
                            }
                            break;
                        case LifetimeScope.InstancePerOwned:
                            if (overrideAttr.Owned is null)
                                throw new InvalidOperationException("Owned type was null");
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerOwned(overrideAttr.Owned);
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerOwned(overrideAttr.Owned);
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerOwned(overrideAttr.Owned);
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerOwned(overrideAttr.Owned);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    switch (config.DataServiceLifetimeScope)
                    {
                        case LifetimeScope.Singleton:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .SingleInstance();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .SingleInstance();
                            }
                            else
                            {
                                if (dataType.IsGenericType)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .SingleInstance();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .SingleInstance();
                            }
                            break;
                        case LifetimeScope.InstancePerRequest:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerRequest();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerRequest();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerRequest();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerRequest();
                            }
                            break;
                        case LifetimeScope.InstancePerLifetimeScope:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerLifetimeScope();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerLifetimeScope();
                            }
                            break;
                        case LifetimeScope.InstancePerDependancy:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerDependency();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerDependency();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerDependency();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerDependency();
                            }
                            break;
                        case LifetimeScope.InstancePerMatchingLifetimeScope:
                            if (dataIsIntercepted)
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerMatchingLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .EnableInterfaceInterceptors()
                                        .InstancePerMatchingLifetimeScope();
                            }
                            else
                            {
                                if (dataType.IsGenericType && dataType.IsGenericTypeDefinition)
                                    builder.RegisterGeneric(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerMatchingLifetimeScope();
                                else
                                    builder.RegisterType(dataType)
                                        .AsImplementedInterfaces()
                                        .InstancePerMatchingLifetimeScope();
                            }
                            break;
                        case LifetimeScope.InstancePerOwned:
                            throw new NotSupportedException();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (var type in subSet)
            {
                var intrAttrs = type.GetCustomAttributes<AutofacInterceptedByAttribute>(false).ToList();
                var scopeAttr = type.GetCustomAttribute<AutofacLifetimeScopeAttribute>();
                var asAttrs = type.GetCustomAttributes<AutofacRegisterAsAttribute>().ToList();

                var scope = scopeAttr?.Scope ?? LifetimeScope.InstancePerLifetimeScope;

                var registerAsTypes = asAttrs.Where(x => x.RegisterAsType is not null).Select(x => x.RegisterAsType).ToList();
                var shouldAsSelf = !asAttrs.Any() || asAttrs.Any(x => x.RegisterAsOption == RegisterAs.AsSelf);
                var shouldAsInterfaces = asAttrs.Any(x => x.RegisterAsOption == RegisterAs.AsImplementedInterfaces);

                bool isIntercepted = intrAttrs.Any();

                IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>? registrationGenericBuilder = null;
                IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>? registrationBuilder = null;

                if (type.IsGenericType && type.IsGenericTypeDefinition)
                {
                    registrationGenericBuilder = shouldAsInterfaces
                        ? builder.RegisterGeneric(type).AsImplementedInterfaces()
                        : builder.RegisterGeneric(type);
                }
                else
                {
                    registrationBuilder = shouldAsInterfaces
                        ? builder.RegisterType(type).AsImplementedInterfaces()
                        : builder.RegisterType(type);
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
                    case LifetimeScope.Singleton:
                        registrationBuilder = registrationBuilder?.SingleInstance();
                        registrationGenericBuilder = registrationGenericBuilder?.SingleInstance();
                        break;
                    case LifetimeScope.InstancePerRequest:
                        registrationBuilder = registrationBuilder?.InstancePerRequest();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerRequest();
                        break;
                    case LifetimeScope.InstancePerLifetimeScope:
                        registrationBuilder = registrationBuilder?.InstancePerLifetimeScope();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerLifetimeScope();
                        break;
                    case LifetimeScope.InstancePerDependancy:
                        registrationBuilder = registrationBuilder?.InstancePerDependency();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerDependency();
                        break;
                    case LifetimeScope.InstancePerMatchingLifetimeScope:
                        registrationBuilder =
                            registrationBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags.ToArray() ??
                                Array.Empty<object>());
                        registrationGenericBuilder =
                            registrationGenericBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags.ToArray() ??
                                Array.Empty<object>());
                        break;
                    case LifetimeScope.InstancePerOwned:
                        if (scopeAttr?.Owned is null) throw new InvalidOperationException("Owned type was null");

                        registrationBuilder = registrationBuilder?.InstancePerOwned(scopeAttr.Owned);
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerOwned(scopeAttr.Owned);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (isIntercepted)
                {
                    registrationBuilder = registrationBuilder?.EnableInterfaceInterceptors();
                    registrationGenericBuilder = registrationGenericBuilder?.EnableInterfaceInterceptors();

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
            /*builder.RegisterTypes(subSet.ToArray())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();*/
        }
    }
}