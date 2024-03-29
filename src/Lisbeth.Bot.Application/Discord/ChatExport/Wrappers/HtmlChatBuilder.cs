// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Lisbeth.Bot.Domain;

// ReSharper disable MemberCanBePrivate.Global

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;

public class HtmlChatBuilder : IAsyncHtmlBuilder
{
    public HtmlChatBuilder()
    {
    }

    public HtmlChatBuilder(List<DiscordUser>? users, List<DiscordMessage>? messages, DiscordChannel? channel,
        string? js, string? css, BotConfiguration? options, DiscordGuild? guild)
    {
        Users = users ?? throw new ArgumentNullException(nameof(users));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Js = js ?? throw new ArgumentNullException(nameof(js));
        Css = css ?? throw new ArgumentNullException(nameof(css));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Guild = guild ?? throw new ArgumentNullException(nameof(guild));
    }

    public List<DiscordUser>? Users { get; private set; }
    public List<DiscordMessage>? Messages { get; private set; }
    public DiscordChannel? Channel { get; private set; }
    public string? Js { get; private set; }
    public string? Css { get; private set; }
    public BotConfiguration? Options { get; private set; }
    public DiscordGuild? Guild { get; private set; }

    public async Task<string> BuildAsync()
    {
        if (Users is null || Messages is null || Channel is null || Js is null || Css is null)
            throw new ArgumentException("You must provide all required parameters before building.");

        MessagesHtmlWrapperBuilder messagesBuilder = new(Messages, Options ?? throw new InvalidOperationException("Options were null"));
        MembersHtmlWrapperBuilder membersBuilder = new(Users, Options);

        return "<!DOCTYPE html>" +
               "<html lang=\"en\">" +
               "<head>" +
               $"<title>{Guild?.Name} - {Channel.Name}</title>" +
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

    public HtmlChatBuilder WithUsers(List<DiscordUser> users)
    {
        Users ??= users ?? throw new ArgumentNullException(nameof(users));

        return this;
    }

    public HtmlChatBuilder WithMessages(List<DiscordMessage> messages)
    {
        Messages ??= messages ?? throw new ArgumentNullException(nameof(messages));

        return this;
    }

    public HtmlChatBuilder WithChannel(DiscordChannel channel)
    {
        Channel ??= channel ?? throw new ArgumentNullException(nameof(channel));

        return this;
    }

    public HtmlChatBuilder WithJs(string js)
    {
        Js ??= js ?? throw new ArgumentNullException(nameof(js));

        return this;
    }

    public HtmlChatBuilder WithGuild(DiscordGuild guild)
    {
        Guild ??= guild ?? throw new ArgumentNullException(nameof(guild));

        return this;
    }

    public HtmlChatBuilder WithCss(string css)
    {
        Css ??= css ?? throw new ArgumentNullException(nameof(css));

        return this;
    }

    public HtmlChatBuilder WithOptions(BotConfiguration configuration)
    {
        Options ??= configuration ?? throw new ArgumentNullException(nameof(configuration));

        return this;
    }
}
