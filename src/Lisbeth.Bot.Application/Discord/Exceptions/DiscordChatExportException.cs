using System;

namespace Lisbeth.Bot.Application.Discord.Exceptions
{
    public class DiscordChatExportException : Exception
    {
        public DiscordChatExportException()
        {
        }

        public DiscordChatExportException(string message)
            : base(message)
        {
        }

        public DiscordChatExportException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
