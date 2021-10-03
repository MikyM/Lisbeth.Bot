using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class MembersHtmlBuilder : IAsyncHtmlBuilder
    {
        public List<DiscordUser> Users { get; private set; }

        public MembersHtmlBuilder(List<DiscordUser> users)
        {
            Users ??= users ?? throw new ArgumentNullException(nameof(users));
        }

        public MembersHtmlBuilder WithUsers(List<DiscordUser> users)
        {
            Users ??= users ?? throw new ArgumentNullException(nameof(users));
            return this;
        }

        public async Task<string> BuildAsync()
        {
            if (Users == null || Users.Count == 0)
            {
                return "";
            }

            string usersHtml = "";
            foreach (var user in Users)
            {
                HtmlUser userModel = new HtmlUser(user);
                usersHtml += $"<div class=\"user\">{await userModel.BuildAvatar()} {userModel.Build()}</div>";
            }
            return $"<div id=\"users-wrapper\">{usersHtml}</div>";
        }
    }
}