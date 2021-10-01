using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class User
    {
        private DiscordUser Usr { get; set; }
        private string AvatarUrl { get; set; }

        public User(DiscordUser user)
        {
            this.Usr = user;
        }

        public string GetHtml()
        {
            return $"<div class=\"user-info\">{this.Usr.Username}#{this.Usr.Discriminator} ID: {this.Usr.Id}</div>";
        }

        public async Task<string> GetAvatarHtml()
        {
            Image avatar = new Image(this.Usr.AvatarUrl);
            string url = await avatar.GetImgurLink();
            this.AvatarUrl = url;
            return $"<img class=\"avatar\" src={url}/>";
        }
    }
}