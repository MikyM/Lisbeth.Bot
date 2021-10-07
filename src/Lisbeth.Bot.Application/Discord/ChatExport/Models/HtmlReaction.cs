using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlReaction
    {
        public DiscordEmoji Emoji { get; private set; }
        public int Count { get; private set; }

        public HtmlReaction(DiscordEmoji emoji, int count)
        {
            Emoji = emoji;
            Count = count;
        }

        public string Build()
        {
            return Emoji.Name.Length is 2 or 1 ? $"<div class=\"reaction-wrapper\">{Emoji.Name} {Count}</div>" : $"<div class=\"reaction-wrapper\"><img class=\"reaction\" src=\"{Emoji.Url}\">{Count}</div>";
        }
    }
}