// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Lisbeth.Bot.API.ExceptionMiddleware;
using Lisbeth.Bot.API.Helpers;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MikyM.Discord.EmbedBuilders;
using Serilog;
using Serilog.Events;

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

            Log.Information("Loading configuration");
            
            // Read shorteners
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "Shorteners.txt");
            string[] shorteners;
            if (File.Exists(path))
            {
                shorteners = (await File.ReadAllTextAsync(path)).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                throw new IOException($"Shorteners file was not found at {path}");
            }
            // Read chat export files
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "ChatExport.css");
            string chatExportCss;
            if (File.Exists(path))
            {
                chatExportCss = (await File.ReadAllTextAsync(path)).Trim().Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);
            }
            else
            {
                throw new IOException($"CSS file was not found at {path}");
            }

            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "ChatExport.js");
            string chatExportJs;
            if (File.Exists(path))
            {
                chatExportJs = (await File.ReadAllTextAsync(path)).Trim().Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);
            }
            else
            {
                throw new IOException($"JS file was not found at {path}");
            }

            var builder = WebApplication.CreateBuilder(args);

            // Set culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            // Configuration
            builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.Configuration.AddJsonFile(
                builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();

            Log.Information("Starting web host");
            
            // Configure some services with base Microsoft DI
            builder.Services.AddControllers(options =>
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
            builder.Services.ConfigureSwagger();
            builder.Services.AddHttpClient();
            builder.Services.ConfigureLisbethDbContext(builder.Configuration, builder.Environment);
            builder.Services.ConfigureDiscord(builder.Configuration);
            builder.Services.ConfigureHangfire(builder.Configuration, builder.Environment);
            builder.Services.ConfigureApiKey(builder.Configuration);
            builder.Services.ConfigureRateLimiting(builder.Configuration);
            builder.Services.ConfigureEfCache();
            builder.Services.ConfigureApiVersioning();
            builder.Services.ConfigureHealthChecks(builder.Configuration, builder.Environment);
            builder.Services.ConfigureFluentValidation();
            builder.Services.AddEnrichedDiscordEmbedBuilders();
            builder.Services.ConfigurePhishingGateway();
            builder.Services.ConfigureBotOptions();

            builder.Services.AddHostedService<InitializationService>();
            
            // Configure Autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
                cb.RegisterModule(new AutofacContainerModule()));

            // Configure Serilog
            builder.Host.UseSerilog((hostBuilder, services, configuration) => configuration
                .ReadFrom.Configuration(hostBuilder.Configuration)
                .ReadFrom.Services(services));

            var app = builder.Build();

            // Set shorteners and chat export files
            var options = app.Services.GetAutofacRoot().Resolve<IOptions<BotConfiguration>>().Value;
            options.SetShorteners(shorteners);
            options.SetChatExportCss(chatExportCss);
            options.SetChatExportJs(chatExportJs);

            // Configure IdGen factory
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
                    ? new DashboardOptions { AppPath = null, Authorization = new[] { new HangfireAlwaysAuthFilter() } }
                    : new DashboardOptions { AppPath = null, Authorization = new[] { new HangfireAuthFilter() } });
            app.UseMiddleware<CustomExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
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
            RecurringJobHelper.ScheduleAllDefinedAfterDelayAsync();

            await app.RunAsync(_cts.Token);
            await Task.Delay(TimeSpan.FromSeconds(15));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
