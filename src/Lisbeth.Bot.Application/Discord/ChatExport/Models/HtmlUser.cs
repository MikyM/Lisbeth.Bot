using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlUser
    {
        public HtmlUser(DiscordUser user)
        {
            User ??= user ?? throw new ArgumentNullException(nameof(user));
        }

        public DiscordUser User { get; }

        public string Build()
        {
            if (User  is null) return "";

            return $"<div class=\"user-info\">{User.Username}#{User.Discriminator} ID: {User.Id}</div>";
        }

        public async Task<string> BuildAvatar()
        {
            if (User  is null) return "";

            HtmlImage avatar;
            string url;
            avatar = new HtmlImage(User.AvatarUrl);
            url = await avatar.GetImgurLink();

            return $"<img class=\"avatar\" src={url}/>";
        }
    }
}