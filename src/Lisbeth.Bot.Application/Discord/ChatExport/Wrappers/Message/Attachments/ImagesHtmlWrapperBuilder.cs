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
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message.Attachments;

public class ImagesHtmlWrapperBuilder : IAsyncHtmlBuilder
{
    public ImagesHtmlWrapperBuilder(IReadOnlyList<DiscordAttachment> images, BotConfiguration configuration)
    {
        Images ??= images ?? throw new ArgumentNullException(nameof(images));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IReadOnlyList<DiscordAttachment> Images { get; private set; }
    public BotConfiguration BotConfiguration { get; private set; }

    public async Task<string> BuildAsync()
    {
        if (Images.Count == 0) return "";
        string imagesHtml = "";
        foreach (var attachment in Images)
        {
            HtmlImage image = new (attachment.Url,BotConfiguration);
            imagesHtml += await image.BuildAsync();
        }

        return $"<div class=\"images-wrapper\">{imagesHtml}</div>";
    }

    public ImagesHtmlWrapperBuilder WithImages(IReadOnlyList<DiscordAttachment> images)
    {
        Images = images ?? throw new ArgumentNullException();

        return this;
    }

    public ImagesHtmlWrapperBuilder WithOptions(BotConfiguration configuration)
    {
        BotConfiguration = configuration ?? throw new ArgumentNullException();

        return this;
    }
}
