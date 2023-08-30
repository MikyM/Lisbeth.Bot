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
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;

public class MembersHtmlWrapperBuilder : IAsyncHtmlBuilder
{
    public MembersHtmlWrapperBuilder(List<DiscordUser> users, BotConfiguration configuration)
    {
        Users ??= users ?? throw new ArgumentNullException(nameof(users));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public List<DiscordUser> Users { get; private set; }
    public BotConfiguration BotConfiguration { get; }

    public async Task<string> BuildAsync()
    {
        if (Users.Count == 0) return "";

        string usersHtml = "";
        foreach (var user in Users)
        {
            HtmlUser userModel = new (user, BotConfiguration);
            usersHtml += $"<div class=\"user\">{await userModel.BuildAvatar()} {await userModel.BuildAsync()}</div>";
        }

        return $"<div id=\"users-wrapper\">{usersHtml}</div>";
    }

    public MembersHtmlWrapperBuilder WithUsers(List<DiscordUser> users)
    {
        Users = users ?? throw new ArgumentNullException(); ;
        return this;
    }
}
