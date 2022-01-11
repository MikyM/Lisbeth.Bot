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

        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>? registReadOnlyBuilder = null;
        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>? registCrudBuilder = null;

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
                    .Any(y => y.GetType() == typeof(AutofacLifetimeScopeAttribute)) && x.IsClass && !x.IsAbstract)
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
                var intrAttr = type.GetCustomAttributes<InterceptAttribute>(false).ToList();
                var attr = type.GetCustomAttribute<AutofacLifetimeScopeAttribute>();

                if (attr is null)
                    throw new InvalidOperationException("Something went wrong with autofac registrations");

                bool isIntercepted = intrAttr.Any();
                switch (attr.Scope)
                {
                    case LifetimeScope.Singleton:
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .SingleInstance();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .SingleInstance();
                        }
                        else
                        {
                            if (type.IsGenericType)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .SingleInstance();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .SingleInstance();
                        }
                        break;
                    case LifetimeScope.InstancePerRequest:
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerRequest();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerRequest();
                        }
                        else
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerRequest();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerRequest();
                        }
                        break;
                    case LifetimeScope.InstancePerLifetimeScope:
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerLifetimeScope();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerLifetimeScope();

                        }
                        else
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerLifetimeScope();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerLifetimeScope();
                        }
                        break;
                    case LifetimeScope.InstancePerDependancy:
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerDependency();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerDependency();
                        }
                        else
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerDependency();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerDependency();
                        }
                        break;
                    case LifetimeScope.InstancePerMatchingLifetimeScope:
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerMatchingLifetimeScope();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerMatchingLifetimeScope();
                        }
                        else
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerMatchingLifetimeScope();
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerMatchingLifetimeScope();
                        }
                        break;
                    case LifetimeScope.InstancePerOwned:
                        if (attr.Owned is null)
                            throw new InvalidOperationException("Owned type was null");
                        if (isIntercepted)
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerOwned(attr.Owned);
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableInterfaceInterceptors()
                                    .InstancePerOwned(attr.Owned);
                        }
                        else
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                                builder.RegisterGeneric(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerOwned(attr.Owned);
                            else
                                builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .InstancePerOwned(attr.Owned);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            /*builder.RegisterTypes(subSet.ToArray())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();*/
        }
    }
}