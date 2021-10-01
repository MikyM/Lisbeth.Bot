using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class Reaction
    {
        private DiscordEmoji Emoji { get; set; }
        private int Amount { get; set; }

        public Reaction(DiscordEmoji emoji, int amount)
        {
            this.Emoji = emoji;
            this.Amount = amount;
        }

        public string GetHtml()
        {
            if (this.Emoji.Name.Length == 2 | this.Emoji.Name.Length == 1)
            {
                return $"<div class=\"reaction-wrapper\">{this.Emoji.Name} {this.Amount}</div>";
            }
            return $"<div class=\"reaction-wrapper\"><img class=\"reaction\" src=\"{this.Emoji.Url}\">{this.Amount}</div>";
        }
    }
}