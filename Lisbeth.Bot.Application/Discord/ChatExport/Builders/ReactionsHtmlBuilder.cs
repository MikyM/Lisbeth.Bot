using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class ReactionsHtmlBuilder : IAsyncHtmlBuilder
    {
        public List<DiscordReaction> Reactions { get; private set; }

        public ReactionsHtmlBuilder(List<DiscordReaction> reactions)
        {
            Reactions ??= reactions ?? throw new ArgumentNullException(nameof(reactions));
        }

        public ReactionsHtmlBuilder WithReactions(List<DiscordReaction> reactions)
        {
            Reactions ??= reactions ?? throw new ArgumentNullException(nameof(reactions));

            return this;
        }

        public Task<string> BuildAsync()
        {
            //Dictionary<DiscordReaction, int> reactions = Reactions.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            string html = "";

            foreach (var item in Reactions)
            {
                HtmlReaction reaction = new HtmlReaction(item.Emoji, item.Count);
                html += reaction.Build();
            }

            return Task.FromResult($"<div class=\"message-reactions\">{html}</div>");
        }
    }
}