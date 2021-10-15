using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlReaction
    {
        public HtmlReaction(DiscordEmoji emoji, int count)
        {
            Emoji = emoji;
            Count = count;
        }

        public DiscordEmoji Emoji { get; }
        public int Count { get; }

        public string Build()
        {
            return Emoji.Name.Length is 2 or 1
                ? $"<div class=\"reaction-wrapper\">{Emoji.Name} {Count}</div>"
                : $"<div class=\"reaction-wrapper\"><img class=\"reaction\" src=\"{Emoji.Url}\">{Count}</div>";
        }
    }
}