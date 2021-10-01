using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class MessagesBuilder
    {
        private List<DiscordMessage> Messages { get; set; }

        public MessagesBuilder(List<DiscordMessage> messages)
        {
            this.Messages = messages;
        }

        public async Task<string> GetHtml()
        {
            if (this.Messages.Count == 0 || this.Messages == null)
            {
                return "";
            }
            string messagesHtml = "";
            foreach (var msg in this.Messages)
            {
                if (msg.Author.IsBot)
                {
                    continue;
                }
                Message message = new Message(msg);
                messagesHtml += await message.GetHtml();
            }
            return $"<div id=\"messages-wrapper\">{messagesHtml}</div>";
        }
    }
}