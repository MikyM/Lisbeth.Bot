// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using System.Collections.Generic;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message;

public class ReactionsHtmlWrapperBuilder : IAsyncHtmlBuilder
{
    public ReactionsHtmlWrapperBuilder() : this(new List<DiscordReaction>())
    {
    }

    public ReactionsHtmlWrapperBuilder(List<DiscordReaction> reactions)
    {
        Reactions ??= reactions ?? throw new ArgumentNullException(nameof(reactions));
    }

    public List<DiscordReaction> Reactions { get; private set; }

    public Task<string> BuildAsync()
    {
        //Dictionary<DiscordReaction, int> reactions = Reactions.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var html = "";

        foreach (var item in Reactions)
        {
            HtmlReaction reaction = new (item.Emoji, item.Count);
            html += reaction.BuildAsync();
        }

        return Task.FromResult($"<div class=\"message-reactions\">{html}</div>");
    }

    public ReactionsHtmlWrapperBuilder WithReactions(List<DiscordReaction> reactions)
    {
        Reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));

        return this;
    }
}
