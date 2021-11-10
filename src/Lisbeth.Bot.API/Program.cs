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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MikyM.Common.Domain;
using Serilog;
using Serilog.Events;
using System;
using System.Globalization;

namespace Lisbeth.Bot.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

                builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                if (builder.Environment.IsProduction()) builder.Configuration.AddJsonFile("appsettings.json", false, true);
                builder.Configuration.AddJsonFile("appsettings.Development.json", true, true);
                builder.Configuration.AddEnvironmentVariables();

                builder.Services.AddControllers(options =>
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
                builder.Services.ConfigureSwagger();
                builder.Services.AddHttpClient();
                builder.Services.ConfigureDiscord();
                builder.Services.ConfigureHangfire();
                builder.Services.ConfigureApiKey(builder.Configuration);
                builder.Services.ConfigureRateLimiting(builder.Configuration);
                builder.Services.ConfigureEfCache();
                builder.Services.ConfigureApiVersioning();
                builder.Services.ConfigureHealthChecks();
                builder.Services.ConfigureFluentValidation();

                builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
                builder.Host.ConfigureContainer<ContainerBuilder>(cb => cb.RegisterModule(new AutofacContainerModule()));
                builder.Host.UseSerilog((hostBuilder, services, configuration) => configuration
                    .ReadFrom.Configuration(hostBuilder.Configuration)
                    .ReadFrom.Services(services));

                builder.WebHost.UseSentry();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                Log.Logger.Debug("Waiting for discord's guild download completion.");
                //WaitForDownloadCompletion.ReadyToOperateEvent.WaitAsync().Wait();
                Log.Logger.Debug("Discord fully operational.");

                ContainerProvider.Container = app.Services.GetAutofacRoot();
                GlobalConfiguration.Configuration.UseAutofacActivator(app.Services.GetAutofacRoot());
                _ = ContainerProvider.Container
                    .Resolve<IAsyncExecutor>()
                    .ExecuteAsync(async () => await RecurringJobHelper.ScheduleAllDefinedAfterDelayAsync());
                
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lisbeth.Bot v1"));
                }

                app.UseHangfireDashboard("/hangfire",
                    app.Environment.IsDevelopment()
                        ? new DashboardOptions { AppPath = "kek", Authorization = new[] { new HangfireAlwaysAuthFilter() } }
                        : new DashboardOptions { AppPath = "kek", Authorization = new[] { new HangfireAuthFilter() } });
                app.UseMiddleware<CustomExceptionMiddleware>();
                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseSentryTracing();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseSerilogRequestLogging();

                app.UseEndpoints(endpoints =>
                {
                    if (app.Environment.IsDevelopment()) endpoints.MapHealthChecks("/health").AllowAnonymous();
                    else endpoints.MapHealthChecks("/health");
                    endpoints.MapControllers();
                });

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}