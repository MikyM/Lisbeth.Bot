using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Http;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using Module = Autofac.Module;

namespace Lisbeth.Bot.API
{
    public class AutofacContainerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            // automapper
            builder.RegisterAutoMapper(opt => opt.AddExpressionMapping(), Assembly.GetExecutingAssembly());
            // unitofwork
            builder.RegisterGeneric(typeof(UnitOfWork<>)).As(typeof(IUnitOfWork<>)).InstancePerLifetimeScope();
            // generic services
            builder.RegisterGeneric(typeof(ReadOnlyService<,>)).As(typeof(IReadOnlyService<,>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(CrudService<,>)).As(typeof(ICrudService<,>))
                .InstancePerLifetimeScope();
            // generic repositories
            builder.RegisterGeneric(typeof(ReadOnlyRepository<>)).As(typeof(IReadOnlyRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>))
                .InstancePerLifetimeScope();
            // bulk register custom services - follow naming convention
            builder.RegisterAssemblyTypes(typeof(SampleService).Assembly).Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            // bulk register custom repositories - follow naming convention
            builder.RegisterAssemblyTypes(typeof(SampleRepository).Assembly).Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // pagination stuff
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.Register(x =>
            {
                var accessor = x.Resolve<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            }).As<IUriService>().SingleInstance();

        }
    }
}
