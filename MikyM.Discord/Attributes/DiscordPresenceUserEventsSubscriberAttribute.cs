﻿using System;
using MikyM.Discord.Events;

namespace MikyM.Discord.Attributes
{
    /// <summary>
    ///     Marks this class as a receiver of <see cref="IDiscordPresenceUserEventsSubscriber" /> events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DiscordPresenceUserEventsSubscriberAttribute : Attribute
    {
    }
}