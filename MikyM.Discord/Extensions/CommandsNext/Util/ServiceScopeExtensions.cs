using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Discord.Extensions.CommandsNext.Events;

namespace MikyM.Discord.Extensions.CommandsNext.Util
{
    internal static class ServiceScopeExtensions
    {
        public static IList<IDiscordCommandsNextEventsSubscriber> GetDiscordCommandsNextEventsSubscriber(
            this IServiceScope scope
        )
        {
            return scope.ServiceProvider
                .GetServices(typeof(IDiscordCommandsNextEventsSubscriber))
                .Cast<IDiscordCommandsNextEventsSubscriber>()
                .ToList();
        }
    }
}