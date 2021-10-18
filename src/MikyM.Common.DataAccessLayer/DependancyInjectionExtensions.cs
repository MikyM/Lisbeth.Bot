using Autofac;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace MikyM.Common.DataAccessLayer
{
    public static class DependancyInjectionExtensions
    {
        public static void AddDataAccessLayer(this IServiceCollection services)
        {
            services.AddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        }
        
        public static void AddDataAccessLayer(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(ReadOnlyRepository<>)).As(typeof(IReadOnlyRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(UnitOfWork<>)).As(typeof(IUnitOfWork<>)).InstancePerLifetimeScope();
        }
    }
}
