using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using AutoMapper.Extensions.ExpressionMapping;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.Application.Services.Interfaces;
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
            builder.RegisterAssemblyTypes(typeof(MuteService).Assembly).Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            // bulk register custom repositories - follow naming convention
            builder.RegisterAssemblyTypes(typeof(MuteRepository).Assembly).Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // register chat builders
            builder.RegisterAssemblyTypes(typeof(HtmlChatBuilder).Assembly).Where(t => t.Name.EndsWith("ChatBuilder"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // pagination stuff
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.Register(x =>
            {
                var accessor = x.Resolve<IHttpContextAccessor>();
                var request = accessor?.HttpContext?.Request;
                var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
                return new UriService(uri);
            }).As<IUriService>().SingleInstance();

        }
    }
}
