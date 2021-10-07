using System;

namespace Lisbeth.Bot.Application.Discord.Exceptions
{
    public class DiscordNotAuthorizedException : Exception
    {
        public DiscordNotAuthorizedException()
        {
        }

        public DiscordNotAuthorizedException(string message)
            : base(message)
        {
        }

        public DiscordNotAuthorizedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
