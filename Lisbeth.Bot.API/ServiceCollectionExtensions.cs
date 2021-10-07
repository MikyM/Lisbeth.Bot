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

using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using Lisbeth.Bot.Application.Discord.ApplicationCommands;
using Lisbeth.Bot.Application.Discord.EventHandlers;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Discord;
using MikyM.Discord.Extensions.Interactivity;
using MikyM.Discord.Extensions.SlashCommands;
using OpenTracing;
using OpenTracing.Mock;
using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Lisbeth.Bot.API.Helpers;
using Lisbeth.Bot.Application.Helpers;

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
                options.UseMemoryStorage(new MemoryStorageOptions{JobExpirationCheckInterval = TimeSpan.FromMinutes(1)});
            });

            services.AddHangfireServer(options =>
            {
                options.Queues = new[] {"Critical", "TimedModeration", "Reminder"};
                options.Activator = new AutofacJobActivator(ContainerProvider.Container);
            });


        }
    }
}
