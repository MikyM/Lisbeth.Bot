using System;
using MikyM.Discord.Extensions.SlashCommands.Events;

namespace MikyM.Discord.Extensions.SlashCommands.Attributes
{
    /// <summary>
    ///     Marks this class as a receiver of <see cref="IDiscordSlashCommandsEventsSubscriber" /> events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DiscordSlashCommandsEventsSubscriberAttribute : Attribute
    {
    }
}