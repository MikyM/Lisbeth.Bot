using System;
using MikyM.Discord.Events;

namespace MikyM.Discord.Attributes
{
    /// <summary>
    ///     Marks this class as a receiver of <see cref="IDiscordWebSocketEventSubscriber" /> events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DiscordWebSocketEventSubscriberAttribute : Attribute
    {
    }
}