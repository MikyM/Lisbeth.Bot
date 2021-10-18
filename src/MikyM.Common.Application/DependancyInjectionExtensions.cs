using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Services;
using System;

namespace MikyM.Common.Application
{
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
}
