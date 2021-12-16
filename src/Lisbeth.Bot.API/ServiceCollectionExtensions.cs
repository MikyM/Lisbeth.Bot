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

using AspNetCore.Authentication.ApiKey;
using AspNetCoreRateLimit;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using EasyCaching.InMemory;
using EFCoreSecondLevelCacheInterceptor;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Lisbeth.Bot.API.HealthChecks;
using Lisbeth.Bot.Application.Discord.ApplicationCommands;
using Lisbeth.Bot.Application.Discord.EventHandlers;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MikyM.Discord;
using MikyM.Discord.Extensions.Interactivity;
using MikyM.Discord.Extensions.SlashCommands;
using OpenTracing;
using OpenTracing.Mock;
using System.Security.Claims;

namespace Lisbeth.Bot.API;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDiscord(this IServiceCollection services)
    {
        services.AddSingleton<ITracer>(_ => new MockTracer());
        services.AddDiscord(options =>
        {
            options.Token = Environment.GetEnvironmentVariable("LisbethTstToken");
            options.Intents = DiscordIntents.All;
        });
        services.AddDiscordHostedService(true);

        //services.AddDiscordGuildEventsSubscriber<ReadyToOperateHandler>();

        #region commands

        services.AddDiscordSlashCommands(_ => { }, extension =>
        {
            extension?.RegisterCommands<MuteApplicationCommands>(790631933758799912);
            extension?.RegisterCommands<BanApplicationCommands>(790631933758799912);
            extension?.RegisterCommands<TicketSlashCommands>(790631933758799912);
            extension?.RegisterCommands<OwnerUtilSlashCommands>(790631933758799912);
            extension?.RegisterCommands<PruneApplicationCommands>(790631933758799912);
            extension?.RegisterCommands<AdminUtilSlashCommands>(790631933758799912);
            extension?.RegisterCommands<ModUtilSlashCommands>(790631933758799912);
            extension?.RegisterCommands<TagSlashCommands>(790631933758799912);
            extension?.RegisterCommands<ReminderSlashCommands>(790631933758799912);
            extension?.RegisterCommands<EmbedConfigSlashCommands>(790631933758799912);
            extension?.RegisterCommands<RoleMenuSlashCommands>(790631933758799912);
        });
        services.AddDiscordInteractivity(options =>
        {
            options.PaginationBehaviour = PaginationBehaviour.WrapAround;
            options.ResponseBehavior = InteractionResponseBehavior.Ack;
            options.Timeout = TimeSpan.FromMinutes(2);
        });

        #endregion


        #region events

        services.AddDiscordSlashCommandsEventsSubscriber<SlashCommandEventsHandler>();
        services.AddDiscordGuildMemberEventsSubscriber<ModerationEventsHandler>();
        services.AddDiscordMessageEventsSubscriber<ModerationEventsHandler>();
        services.AddDiscordMiscEventsSubscriber<TicketEventsHandler>();
        services.AddDiscordChannelEventsSubscriber<TicketEventsHandler>();
        services.AddDiscordMiscEventsSubscriber<EmbedConfigEventHandler>();
        services.AddDiscordGuildEventsSubscriber<GuildEventsHandler>();
        services.AddDiscordMiscEventsSubscriber<RoleMenuEventHandler>();

        #endregion
    }

    public static void ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(options =>
        {
            options.UseRecommendedSerializerSettings();
            options.UsePostgreSqlStorage(configuration.GetConnectionString("HangfireDb"),
                new PostgreSqlStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(15) });
            //options.UseFilter(new QueueFilter());
            //options.UseFilter(new PreserveOriginalQueueAttribute());
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "critical", "moderation", "reminder", "ticketing", "default" };
        });
    }

    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersion.Default;
        });
    }

    public static void ConfigureApiKey(this IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration.GetValue<string>("ApiKey");
        services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeaderOrQueryParams(options =>
            {
                options.Realm = "Lisbeth.Bot";
                options.KeyName = ApiKeyDefaults.AuthenticationScheme;
                options.Events.OnValidateKey =
                    context =>
                    {
                        var isValid = key.Equals(context.ApiKey, StringComparison.OrdinalIgnoreCase);
                        if (isValid)
                        {
                            context.Principal = new ClaimsPrincipal(new[]
                            {
                                new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.SerialNumber, context.ApiKey),
                                    new Claim(ClaimTypes.Role, "Api")
                                }, ApiKeyDefaults.AuthenticationScheme)
                            });
                            context.Success();
                        }
                        else
                        {
                            context.ValidationFailed();
                        }

                        return Task.CompletedTask;
                    };
            });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });
    }

    public static void ConfigureEfCache(this IServiceCollection services)
    {
        services.AddEFSecondLevelCache(options =>
        {
            options.UseEasyCachingCoreProvider("InMemoryCache").DisableLogging(true).UseCacheKeyPrefix("EF_");
            options.CacheQueriesContainingTypes(
                CacheExpirationMode.Sliding, TimeSpan.FromMinutes(30),
                typeof(Guild)
            );
        });
        services.AddEasyCaching(options =>
        {
            options.UseInMemory(config =>
            {
                config.DBConfig = new InMemoryCachingOptions
                {
                    SizeLimit = 100
                };
                config.EnableLogging = true;
            }, "InMemoryCache");
        });
    }

    public static void ConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "EclipseBot", Version = "v1" });
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = ApiKeyDefaults.AuthenticationScheme
                },
                In = ParameterLocation.Header,
                Description =
                    $"Please enter your api key in the field, prefixed with '{ApiKeyDefaults.AuthenticationScheme} '",
                Name = "Authorization"
            };
            options.AddSecurityDefinition(ApiKeyDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });
    }

    public static void ConfigureBotSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var sp = services.BuildServiceProvider();
        //services.AddOptions<BotSettings>().BindConfiguration(sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment() ? "BotSettings:Dev" : "BotSettings:Prod", options => options.BindNonPublicProperties = true);
    }

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                "User ID=lisbethbot;Password=lisbethbot;Host=localhost;Port=5438;Database=lisbeth_bot_test;",
                name: "Base DB")
            .AddNpgSql(
                "User ID=lisbethbot;Password=lisbethbot;Host=localhost;Port=5438;Database=lisbeth_bot_hangfire_test;",
                name: "Hangfire DB")
            .AddHangfire(options =>
            {
                options.MinimumAvailableServers = 1;
                options.MaximumJobsFailed = 10;
            })
            .AddCheck<DiscordHealthCheck>("Discord health check");
    }

    public static void ConfigureFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
            options.AutomaticValidationEnabled = false;
            options.ImplicitlyValidateChildProperties = true;
        });
    }
}