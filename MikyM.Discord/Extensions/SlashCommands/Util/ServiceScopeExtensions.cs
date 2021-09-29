using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Discord.Extensions.SlashCommands.Events;

namespace MikyM.Discord.Extensions.SlashCommands.Util
{
    internal static class ServiceScopeExtensions
    {
        public static IList<IDiscordSlashCommandsEventsSubscriber> GetDiscordSlashCommandsEventsSubscriber(
            this IServiceScope scope
        )
        {
            return scope.ServiceProvider
                .GetServices(typeof(IDiscordSlashCommandsEventsSubscriber))
                .Cast<IDiscordSlashCommandsEventsSubscriber>()
                .ToList();
        }
    }
}