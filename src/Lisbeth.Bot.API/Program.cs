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
using IdGen;
using Lisbeth.Bot.API.ExceptionMiddleware;
using Lisbeth.Bot.API.Helpers;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MikyM.Common.Domain;
using MikyM.Discord.EmbedBuilders;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.API;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    // ReSharper disable once InconsistentNaming
    private static readonly CancellationTokenSource _cts = new ();

    public static async Task Main(string[] args)
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting web host");

            var builder = WebApplication.CreateBuilder(args);

            // Set culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            // Configuration
            builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.Configuration.AddJsonFile(
                builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();

            // Configure some services with base Microsoft DI
            builder.Services.AddControllers(options =>
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
            builder.Services.ConfigureSwagger();
            builder.Services.AddHttpClient();
            builder.Services.ConfigureDiscord(builder.Configuration);
            builder.Services.ConfigureHangfire(builder.Configuration);
            builder.Services.ConfigureApiKey(builder.Configuration);
            builder.Services.ConfigureRateLimiting(builder.Configuration);
            builder.Services.ConfigureEfCache();
            builder.Services.ConfigureApiVersioning();
            builder.Services.ConfigureHealthChecks();
            builder.Services.ConfigureFluentValidation();
            builder.Services.AddEnrichedDiscordEmbedBuilders();
            builder.Services.AddOptions<BotOptions>()
                .BindConfiguration("BotOptions", options => options.BindNonPublicProperties = true);

            // Configure Autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
                cb.RegisterModule(new AutofacContainerModule()));

            // Configure Serilog
            builder.Host.UseSerilog((hostBuilder, services, configuration) => configuration
                .ReadFrom.Configuration(hostBuilder.Configuration)
                .ReadFrom.Services(services));

            // Configure Sentry
            builder.WebHost.UseSentry();

            var app = builder.Build();

            // Configure IdGen factory
            IdGeneratorFactory.SetFactory(() => app.Services.GetAutofacRoot().Resolve<IdGenerator>());
            ChatExportHttpClientFactory.SetFactory(() =>
                app.Services.GetAutofacRoot().Resolve<IHttpClientFactory>().CreateClient());

            GlobalConfiguration.Configuration.UseAutofacActivator(app.Services.GetAutofacRoot());

            // Configure the HTTP request pipeline.
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

            // Schedule recurring jobs
            await RecurringJobHelper.ScheduleAllDefinedAfterDelayAsync();

            await app.RunAsync(_cts.Token);
            await Task.Delay(TimeSpan.FromSeconds(15));
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