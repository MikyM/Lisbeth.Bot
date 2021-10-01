using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class Message
    {
        private DiscordMessage Msg { get; set; }

        public Message(DiscordMessage message)
        {
            this.Msg = message;
        }

        public async Task<string> GetHtml()
        {
            DiscordMember member;
            string messageTop = "";
            string messageBot = "";
            string attachmentsHtml = "";
            string reactionsHtml = "";

            try
            {
                member = await Program.guild.GetMemberAsync(this.Msg.Author.Id);
                messageTop = $"<span class=\"nickname\">{member.DisplayName}</span> <span class=\"message-info-details\">{this.Msg.Timestamp} Message ID: {this.Msg.Id}</span>";
            }
            catch
            {
                messageTop = $"<span class=\"nickname\">{this.Msg.Author.Username}</span> <span class=\"message-info-details\">{this.Msg.Timestamp} Message ID: {this.Msg.Id}</span>";
            }
            messageTop = $"<div class=\"message-info\">{messageTop}</div>";

            if (this.Msg.Content != "" && this.Msg.Content is not null)
            {
                MarkdownParser parser = new MarkdownParser(this.Msg.Content);
                messageBot = await parser.GetParsedContent();
                messageBot = $"<div class=\"message-content\">{messageBot}</div>";
            }

            if (this.Msg.Attachments.Count != 0)
            {
                AttachmentsBuilder attachmentsBuilder = new AttachmentsBuilder(this.Msg.Attachments);
                attachmentsHtml = await attachmentsBuilder.GetHtml();
            }

            if (this.Msg.Reactions.Count != 0)
            {
                ReactionsBuilder reactionsBuilder = new ReactionsBuilder(this.Msg.Reactions.ToList());
                reactionsHtml = reactionsBuilder.GetHtml();
            }

            return $"<div class=\"message\">{messageTop}{messageBot}{attachmentsHtml}{reactionsHtml}</div><hr>";
        }
    }
}