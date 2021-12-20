// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message.Attachments;

public class VideosHtmlWrapperBuilder : IAsyncHtmlBuilder
{

    public VideosHtmlWrapperBuilder(IReadOnlyList<DiscordAttachment> videos, BotOptions options)
    {
        Videos ??= videos ?? throw new ArgumentNullException(nameof(videos));
        Options ??= options ?? throw new ArgumentNullException(nameof(options));
    }

    public IReadOnlyList<DiscordAttachment> Videos { get; private set; }
    public BotOptions Options { get; private set; }

    public async Task<string> BuildAsync()
    {
        if (Videos.Count == 0) return "";
        string videosHtml = "";
        foreach (var attachment in Videos)
        {
            HtmlVideo video = new(attachment.Url, Options);
            videosHtml += await video.BuildAsync();
        }

        return $"<div class=\"videos-wrapper\">{videosHtml}</div>";
    }

    public VideosHtmlWrapperBuilder WithVideos(IReadOnlyList<DiscordAttachment> videos)
    {
        Videos = videos ?? throw new ArgumentNullException(nameof(videos));

        return this;
    }
}