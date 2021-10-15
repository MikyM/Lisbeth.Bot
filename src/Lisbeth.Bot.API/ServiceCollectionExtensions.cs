// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using AspNetCoreRateLimit;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using EasyCaching.InMemory;
using EFCoreSecondLevelCacheInterceptor;
using Hangfire;
using Hangfire.MemoryStorage;
using Lisbeth.Bot.Application.Discord.ApplicationCommands;
using Lisbeth.Bot.Application.Discord.EventHandlers;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.Domain.Entities;
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

namespace Lisbeth.Bot.API
{
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
            services.AddDiscordHostedService();

            #region commands

            services.AddDiscordSlashCommands(_ => { }, extension =>
            {
                extension.RegisterCommands<MuteApplicationCommands>(790631933758799912);
                extension.RegisterCommands<BanApplicationCommands>(790631933758799912);
                extension.RegisterCommands<TicketSlashCommands>(790631933758799912);
                extension.RegisterCommands<AdminUtilSlashCommands>(790631933758799912);
                extension.RegisterCommands<PruneApplicationCommands>(790631933758799912);
                extension.RegisterCommands<ModerationUtilSlashCommands>(790631933758799912);
            });
            services.AddDiscordInteractivity(options =>
            {
                options.PaginationBehaviour = PaginationBehaviour.WrapAround;
                options.ResponseBehavior = InteractionResponseBehavior.Ack;
                options.AckPaginationButtons = true;
                options.Timeout = TimeSpan.FromMinutes(2);
            });

            #endregion


            #region events

            services.AddDiscordSlashCommandsEventsSubscriber<SlashCommandEventsHandler>();
            services.AddDiscordGuildMemberEventsSubscriber<ModerationEventsHandler>();
            services.AddDiscordMessageEventsSubscriber<ModerationEventsHandler>();
            services.AddDiscordMiscEventsSubscriber<TicketEventsHandler>();

            #endregion
        }

        public static void ConfigureHangfire(this IServiceCollection services)
        {
            services.AddHangfire(options =>
            {
                options.UseRecommendedSerializerSettings();
/*                options.UsePostgreSqlStorage(Environment.GetEnvironmentVariable("HangfireTstConnection"),
                    new PostgreSqlStorageOptions {QueuePollInterval = TimeSpan.FromSeconds(15)});*/
                options.UseMemoryStorage(
                    new MemoryStorageOptions {JobExpirationCheckInterval = TimeSpan.FromMinutes(1)});
            });

            services.AddHangfireServer(options =>
                options.Queues = new[] {"critical", "moderation", "reminder", "default"});
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
                .AddApiKeyInAuthorizationHeader(options =>
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
                options.UseEasyCachingCoreProvider("InMemoryCache").DisableLogging().UseCacheKeyPrefix("EF_");
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
                options.SwaggerDoc("v1", new OpenApiInfo {Title = "EclipseBot", Version = "v1"});
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
                    {securityScheme, Array.Empty<string>()}
                });
            });
        }

        public static void ConfigureBotSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var sp = services.BuildServiceProvider();
            //services.AddOptions<BotSettings>().BindConfiguration(sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment() ? "BotSettings:Dev" : "BotSettings:Prod", options => options.BindNonPublicProperties = true);
        }
    }
}