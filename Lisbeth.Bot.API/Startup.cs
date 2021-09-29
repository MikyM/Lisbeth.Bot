using Autofac;
using Lisbeth.Bot.API.Helpers;
using Lisbeth.Bot.DataAccessLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MikyM.Discord;
using OpenTracing;
using OpenTracing.Mock;
using Serilog;

namespace Lisbeth.Bot.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<LisbethBotDbContext>(options =>
                //options.UseNpgsql(Configuration.GetConnectionString("LisbethBotDb")));
            services.AddDbContext<LisbethBotDbContext>(options => options.UseInMemoryDatabase("testDb"));
            services.AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lisbeth.Bot", Version = "v1" });
            });
            services.ConfigureDiscord(Configuration);
        }

        /// <summary>
        /// Configure Container using Autofac: Register DI.
        /// This is called AFTER ConfigureServices.So things you register here OVERRIDE things registered in ConfigureServices.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacContainerModule());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lisbeth.Bot v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSerilogRequestLogging();
        }
    }
}
