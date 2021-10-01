using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class HtmlBuilder
    {
        private List<DiscordUser> Users { get; set; }
        private List<DiscordMessage> Messages { get; set; }
        private DiscordChannel Channel { get; set; }

        public HtmlBuilder(List<DiscordMessage> messages, List<DiscordUser> users, DiscordChannel channel)
        {
            this.Users = users;
            this.Messages = messages;
            this.Channel = channel;
        }

        public async Task<string> Build(string css, string js)
        {
            if (this.Users == null || this.Messages == null || this.Messages.Count == 0 || this.Users.Count == 0 || this.Channel == null)
            {
                return "";
            }

            MessagesBuilder messagesBuilder = new MessagesBuilder(this.Messages);
            UsersBuilder usersBuilder = new UsersBuilder(this.Users);

            string html = "<!DOCTYPE html>" +
                "<html lang=\"en\">" +
                "<head>" +
                    $"<title>Eclipse - {this.Channel.Name}</title>" +
                    "<meta charset=\"utf-8\">" +
                    "<meta name=\"viewport\" content=\"width=device-width\">" +
                    "<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-BmbxuPwQa2lc/FVzBcNJ7UAyJxM6wuqIj61tLrc4wSX0szH/Ev+nYRRuWlolflfl\" crossorigin=\"anonymous\">" +
                $"<style>{css}</style>" +
                "</head>" +
                "<body>" +
                "<div id=\"container\">" +
                $"<h1>{this.Channel.Name} chat log</h1>" +
                $"{await usersBuilder.GetHtml()}" +
                "<h2 style=\"padding-bottom:10px\">Messages</h2>" +
                $"{await messagesBuilder.GetHtml()}" +
                "</div>" +
                "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js\"></script>" +
                $"<script>{js}</script>" +
                "<script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta2/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-b5kHyXgcpbZJO/tY9Ul7kGkf1S0CWuKcCD38l8YkeH8z8QjE0GmW1gYU5S9FOnJ0\" crossorigin=\"anonymous\"></script>" +
                "</body>" +
                "</html>";

            return html;
        }
    }
}