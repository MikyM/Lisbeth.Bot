using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class MessagesHtmlBuilder
    {
        public List<DiscordMessage> Messages { get;private set; }

        public MessagesHtmlBuilder(List<DiscordMessage> messages)
        {
            Messages ??= messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public async Task<string> BuildAsync()
        {
            if (Messages.Count == 0 || Messages == null)
            {
                return "";
            }
            string messagesHtml = "";
            foreach (var msg in Messages)
            {
                if (msg.Author.IsBot)
                {
                    continue;
                }
                HtmlMessage message = new HtmlMessage(msg);
                messagesHtml += await message.Build();
            }
            return $"<div id=\"messages-wrapper\">{messagesHtml}</div>";
        }
    }
}