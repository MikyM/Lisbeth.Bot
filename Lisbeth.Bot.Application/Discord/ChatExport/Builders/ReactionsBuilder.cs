using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class ReactionsBuilder
    {
        private List<DiscordReaction> Reactions { get; set; }

        public ReactionsBuilder(List<DiscordReaction> reactions)
        {
            this.Reactions = reactions;
        }

        public string GetHtml()
        {
            Dictionary<DiscordReaction, int> reactions = this.Reactions.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            string html = "";

            foreach (var item in this.Reactions)
            {
                Reaction reaction = new Reaction(item.Emoji, item.Count);
                html += reaction.GetHtml();
            }

            return $"<div class=\"message-reactions\">{html}</div>";
        }
    }
}