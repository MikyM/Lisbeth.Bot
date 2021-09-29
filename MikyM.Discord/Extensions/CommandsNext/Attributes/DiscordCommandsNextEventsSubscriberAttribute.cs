using System;
using MikyM.Discord.Extensions.CommandsNext.Events;

namespace MikyM.Discord.Extensions.CommandsNext.Attributes
{
    /// <summary>
    ///     Marks this class as a receiver of <see cref="IDiscordCommandsNextEventsSubscriber" /> events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DiscordCommandsNextEventsSubscriberAttribute : Attribute
    {
    }
}