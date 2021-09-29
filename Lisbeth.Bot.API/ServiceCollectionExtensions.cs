using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Discord;
using MikyM.Discord.Extensions.CommandsNext;
using MikyM.Discord.Extensions.Interactivity;
using MikyM.Discord.Extensions.SlashCommands;
using OpenTracing;
using OpenTracing.Mock;

namespace Lisbeth.Bot.API
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDiscord(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITracer>(provider => new MockTracer());
            services.AddDiscord(options =>
            {
                options.Token = "ODU0MzI5OTE5NTU0NDUzNTE0.YMiWvQ.Y3AEfhcNXRPpLNQdQV7WBRiAW_w";
                options.Intents = DiscordIntents.All;
            });
            services.AddDiscordHostedService();

            #region commands
            services.AddDiscordSlashCommands(_ => { }, extension =>
            {
                extension.RegisterCommands<MuteSlashCommands>(790631933758799912);
                extension.RegisterCommands<AdminUtilSlashCommands>(790631933758799912);
            });
            services.AddDiscordInteractivity(options =>
            {
                options.PaginationBehaviour = PaginationBehaviour.WrapAround;
                options.ResponseBehavior = InteractionResponseBehavior.Ack;
                options.AckPaginationButtons = true;
                options.Timeout = TimeSpan.FromMinutes(2);
            });
            services.AddDiscordCommandsNext(options =>
            {
                options.StringPrefixes = new List<string>() { "!" };
                options.CaseSensitive = false;
                options.DmHelp = false;
                options.EnableDms = false;
                options.EnableMentionPrefix = true;
                options.IgnoreExtraArguments = true;
                options.EnableDefaultHelp = false;
            });
            #endregion
            #region events
            //services.AddDiscordMiscEventsSubscriber<BotModuleForMiscEvents>();
            #endregion
        }
    }
}
