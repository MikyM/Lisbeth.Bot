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

using System;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Services;

namespace MikyM.Common.Application;

public static class DependancyInjectionExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped(typeof(IReadOnlyService<,>), typeof(ReadOnlyService<,>));
        services.AddScoped(typeof(CrudService<,>), typeof(CrudService<,>));
        services.AddAutoMapper(x =>
        {
            x.AddExpressionMapping();
            x.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
    }

    public static void AddApplicationLayer(this ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(ReadOnlyService<,>)).As(typeof(IReadOnlyService<,>))
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
            .InstancePerLifetimeScope();
        builder.RegisterAutoMapper(opt => opt.AddExpressionMapping(), AppDomain.CurrentDomain.GetAssemblies());
    }
}