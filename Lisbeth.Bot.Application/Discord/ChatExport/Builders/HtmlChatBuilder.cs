using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class HtmlChatBuilder : IHtmlChatBuilder
    {
        public List<DiscordUser> Users { get; private set; }
        public List<DiscordMessage> Messages { get; private set; }
        public DiscordChannel Channel { get; private set; }
        public string Js { get; private set; }
        public string Css { get; private set; }

        public IHtmlChatBuilder WithUsers(List<DiscordUser> users)
        {
            Users ??= users ?? throw new ArgumentNullException(nameof(users));

            return this;
        }

        public IHtmlChatBuilder WithMessages(List<DiscordMessage> messages)
        {
            Messages ??= messages ?? throw new ArgumentNullException(nameof(messages));

            return this;
        }

        public IHtmlChatBuilder WithChannel(DiscordChannel channel)
        {
            Channel ??= channel ?? throw new ArgumentNullException(nameof(channel));

            return this;
        }

        public IHtmlChatBuilder WithJs(string  js)
        {
            Js ??= js ?? throw new ArgumentNullException(nameof(js));

            return this;
        }

        public IHtmlChatBuilder WithCss(string css)
        {
            Css ??= css ?? throw new ArgumentNullException(nameof(css));

            return this;
        }

        public async Task<string> BuildAsync()
        {
            if (Users is null || Messages is null || Channel is null || Js is null || Css is null)
            {
                throw new ArgumentException("You must provide all required parameters before building.");
            }

            MessagesHtmlBuilder messagesBuilder = new MessagesHtmlBuilder(Messages);
            MembersHtmlBuilder membersBuilder = new MembersHtmlBuilder(Users);

            return "<!DOCTYPE html>" +
                   "<html lang=\"en\">" +
                   "<head>" +
                   $"<title>Eclipse - {Channel.Name}</title>" +
                   "<meta charset=\"utf-8\">" +
                   "<meta name=\"viewport\" content=\"width=device-width\">" +
                   "<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-BmbxuPwQa2lc/FVzBcNJ7UAyJxM6wuqIj61tLrc4wSX0szH/Ev+nYRRuWlolflfl\" crossorigin=\"anonymous\">" +
                   $"<style>{Css}</style>" +
                   "</head>" +
                   "<body>" +
                   "<div id=\"container\">" +
                   $"<h1>{Channel.Name} chat log</h1>" +
                   $"{await membersBuilder.BuildAsync()}" +
                   "<h2 style=\"padding-bottom:10px\">Messages</h2>" +
                   $"{await messagesBuilder.BuildAsync()}" +
                   "</div>" +
                   "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js\"></script>" +
                   $"<script>{Js}</script>" +
                   "<script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta2/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-b5kHyXgcpbZJO/tY9Ul7kGkf1S0CWuKcCD38l8YkeH8z8QjE0GmW1gYU5S9FOnJ0\" crossorigin=\"anonymous\"></script>" +
                   "</body>" +
                   "</html>";
        }
    }
}