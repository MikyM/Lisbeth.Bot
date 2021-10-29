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

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class MembersHtmlBuilder : IAsyncHtmlBuilder
    {
        public MembersHtmlBuilder(List<DiscordUser> users)
        {
            Users ??= users ?? throw new ArgumentNullException(nameof(users));
        }

        public List<DiscordUser> Users { get; private set; }

        public async Task<string> BuildAsync()
        {
            if (Users is null || Users.Count == 0) return "";

            string usersHtml = "";
            foreach (var user in Users)
            {
                HtmlUser userModel = new HtmlUser(user);
                usersHtml += $"<div class=\"user\">{await userModel.BuildAvatar()} {userModel.Build()}</div>";
            }

            return $"<div id=\"users-wrapper\">{usersHtml}</div>";
        }

        public MembersHtmlBuilder WithUsers(List<DiscordUser> users)
        {
            Users ??= users ?? throw new ArgumentNullException(nameof(users));
            return this;
        }
    }
}