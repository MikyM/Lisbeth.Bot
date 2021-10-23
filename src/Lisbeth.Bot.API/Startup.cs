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
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Lisbeth.Bot.API.ExceptionMiddleware;
using Lisbeth.Bot.API.Helpers;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Globalization;

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
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            services.AddControllers(options =>
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
            services.ConfigureSwagger();
            services.AddHttpClient();
            services.ConfigureDiscord();
            services.ConfigureHangfire();
            services.ConfigureApiKey(Configuration);
            services.ConfigureRateLimiting(Configuration);
            services.ConfigureEfCache();
            services.ConfigureApiVersioning();
            services.ConfigureHealthChecks();
            services.ConfigureFluentValidation();
        }

        /// <summary>
        ///     Configure Container using Autofac: Register DI.
        ///     This is called AFTER ConfigureServices.So things you register here OVERRIDE things registered in ConfigureServices.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacContainerModule());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ContainerProvider.Container = app.ApplicationServices.GetAutofacRoot();
            GlobalConfiguration.Configuration.UseAutofacActivator(app.ApplicationServices.GetAutofacRoot());
            _ = ContainerProvider.Container
                .Resolve<IAsyncExecutor>()
                .ExecuteAsync(async () => await RecurringJobHelper.ScheduleAllDefinedAfterDelayAsync());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lisbeth.Bot v1"));
            }

            app.UseMiddleware<CustomExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSentryTracing();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSerilogRequestLogging();

            if (env.IsDevelopment()) app.UseHangfireDashboard();
            else app.UseHangfireDashboard("/hangfire", new DashboardOptions {AppPath = "kek", Authorization = new[] {new HangfireAuthFilter()}});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health").RequireAuthorization();
                endpoints.MapControllers();
            });
        }
    }
}