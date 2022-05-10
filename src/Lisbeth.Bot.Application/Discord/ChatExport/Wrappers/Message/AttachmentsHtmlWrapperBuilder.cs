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
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message.Attachments;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message;

public class AttachmentsHtmlWrapperBuilder : IAsyncHtmlBuilder
{
    public AttachmentsHtmlWrapperBuilder(IReadOnlyList<DiscordAttachment> attachments, BotConfiguration configuration)
    {
        Attachments ??= attachments ?? throw new ArgumentNullException(nameof(attachments));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }
    public BotConfiguration BotConfiguration { get; private set; }

    public async Task<string> BuildAsync()
    {
        if (Attachments.Count == 0) return "";
        var imageAttachments = Attachments
            .Where(a => HtmlImage.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
        var videoAttachments = Attachments
            .Where(a => HtmlVideo.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
        string imagesHtml = "";
        string videosHtml = "";

        if (imageAttachments.Count != 0)
        {
            ImagesHtmlWrapperBuilder imagesBuilder = new(imageAttachments, BotConfiguration);
            imagesHtml = await imagesBuilder.BuildAsync();
        }

        if (videoAttachments.Count == 0) return $"<div class=\"attachments\">{imagesHtml}{videosHtml}</div>";

        VideosHtmlWrapperBuilder videosBuilder = new(videoAttachments, BotConfiguration);
        videosHtml = await videosBuilder.BuildAsync();

        return $"<div class=\"attachments\">{imagesHtml}{videosHtml}</div>";
    }

    public AttachmentsHtmlWrapperBuilder WithAttachments(IReadOnlyList<DiscordAttachment> attachments)
    {
        Attachments = attachments ?? throw new ArgumentNullException(nameof(attachments));

        return this;
    }

    public AttachmentsHtmlWrapperBuilder WithOptions(BotConfiguration configuration)
    {
        BotConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        return this;
    }
}