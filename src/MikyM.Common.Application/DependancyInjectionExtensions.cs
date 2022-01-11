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

using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Common.Application.Services;
using MikyM.Common.Utilities.Autofac;
using MikyM.Common.Utilities.Autofac.Attributes;

namespace MikyM.Common.Application;

public static class DependancyInjectionExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped(typeof(IReadOnlyDataService<,>), typeof(ReadOnlyDataService<,>));
        services.AddScoped(typeof(CrudService<,>), typeof(CrudService<,>));
        services.AddAutoMapper(x =>
        {
            x.AddExpressionMapping();
            x.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
    }

    public static void AddApplicationLayer(this ContainerBuilder builder, LifetimeScope? baseScopeOverride = null)
    {
        if (baseScopeOverride is null)
        {
            builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                .InstancePerLifetimeScope();
        }
        else
        {
            switch (baseScopeOverride)
            {
                case LifetimeScope.Singleton:
                    builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                        .SingleInstance();
                    builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                        .SingleInstance();
                    break;
                case LifetimeScope.InstancePerRequest:
                    builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                        .InstancePerRequest();
                    builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                        .InstancePerRequest();
                    break;
                case LifetimeScope.InstancePerLifetimeScope:
                    builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                        .InstancePerLifetimeScope();
                    builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                        .InstancePerLifetimeScope();
                    break;
                case LifetimeScope.InstancePerMatchingLifetimeScope:
                    builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                        .InstancePerMatchingLifetimeScope();
                    builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                        .InstancePerMatchingLifetimeScope();
                    break;
                case LifetimeScope.InstancePerDependancy:
                    builder.RegisterGeneric(typeof(ReadOnlyDataService<,>)).As(typeof(IReadOnlyDataService<,>))
                        .InstancePerDependency();
                    builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                        .InstancePerDependency();
                    break;
                case LifetimeScope.InstancePerOwned:
                    throw new NotSupportedException();
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(baseScopeOverride), baseScopeOverride, null);
            }
        }


        builder.RegisterAutoMapper(opt => opt.AddExpressionMapping(), false, AppDomain.CurrentDomain.GetAssemblies());

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var subSet = assembly.GetTypes()
                .Where(x => x.GetInterfaces()
                    .Any(y => y == typeof(IServiceBase)) && x.IsClass)
                .ToList();

            var attSubSet = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false)
                    .Any(y => y.GetType() == typeof(AutofacLifetimeScopeAttribute)) && x.IsClass)
                .ToList();

            var typeSet = subSet.Union(attSubSet);

            foreach (var type in typeSet)
            {
                var attr = type.GetCustomAttribute(typeof(AutofacLifetimeScopeAttribute), false);
                if (attr is null)
                    builder.RegisterType(type)
                        .AsImplementedInterfaces()
                        .InstancePerLifetimeScope();
                else
                {
                    var concAttr = (AutofacLifetimeScopeAttribute) attr;
                    switch (concAttr.Scope)
                    {
                        case LifetimeScope.Singleton:
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .SingleInstance();
                            break;
                        case LifetimeScope.InstancePerRequest:
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .InstancePerRequest();
                            break;
                        case LifetimeScope.InstancePerLifetimeScope:
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .InstancePerLifetimeScope();
                            break;
                        case LifetimeScope.InstancePerDependancy:
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .InstancePerDependency();
                            break;
                        case LifetimeScope.InstancePerMatchingLifetimeScope:
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .InstancePerMatchingLifetimeScope();
                            break;
                        case LifetimeScope.InstancePerOwned:
                            if (concAttr.Owned is null)
                                throw new InvalidOperationException("Owned type was null");
                            builder.RegisterType(type)
                                .AsImplementedInterfaces()
                                .InstancePerOwned(concAttr.Owned);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            /*builder.RegisterTypes(subSet.ToArray())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();*/
        }
    }
}