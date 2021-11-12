// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders;

public class MessagesHtmlBuilder : IAsyncHtmlBuilder
{
    public MessagesHtmlBuilder(List<DiscordMessage> messages)
    {
        Messages ??= messages ?? throw new ArgumentNullException(nameof(messages));
    }

    public List<DiscordMessage> Messages { get; }

    public async Task<string> BuildAsync()
    {
        if (Messages is null || Messages.Count == 0) return "";
        string messagesHtml = "";
        foreach (var msg in Messages)
        {
            if (msg.Author.IsBot) continue;
            HtmlMessage message = new HtmlMessage(msg);
            messagesHtml += await message.Build();
        }

        return $"<div id=\"messages-wrapper\">{messagesHtml}</div>";
    }
}