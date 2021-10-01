using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class UsersBuilder
    {
        private List<DiscordUser> Users { get; set; }

        public UsersBuilder(List<DiscordUser> users)
        {
            this.Users = users;
        }

        public async Task<string> GetHtml()
        {
            if (this.Users == null || this.Users.Count == 0)
            {
                return "";
            }
            string usersHtml = "";
            foreach (var user in this.Users)
            {
                User userModel = new User(user);
                usersHtml += $"<div class=\"user\">{await userModel.GetAvatarHtml()} {userModel.GetHtml()}</div>";
            }
            return $"<div id=\"users-wrapper\">{usersHtml}</div>";
        }
    }
}