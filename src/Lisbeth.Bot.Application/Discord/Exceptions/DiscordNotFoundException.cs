using System;

namespace Lisbeth.Bot.Application.Discord.Exceptions
{
    public class DiscordNotFoundException : Exception
    {
        public DiscordNotFoundException()
        {
        }

        public DiscordNotFoundException(string message)
            : base(message)
        {
        }

        public DiscordNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
