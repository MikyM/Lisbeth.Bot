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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class ImagesHtmlBuilder : IAsyncHtmlBuilder
    {
        public ImagesHtmlBuilder(IReadOnlyList<DiscordAttachment> images)
        {
            Images ??= images ?? throw new ArgumentNullException(nameof(images));
        }

        public IReadOnlyList<DiscordAttachment> Images { get; private set; }

        public async Task<string> BuildAsync()
        {
            if (Images.Count == 0 || Images is null) return "";
            string imagesHtml = "";
            foreach (var attachment in Images)
            {
                HtmlImage image = new HtmlImage(attachment.Url);
                imagesHtml += await image.BuildAsync();
            }

            return $"<div class=\"images-wrapper\">{imagesHtml}</div>";
        }

        public ImagesHtmlBuilder WithImages(IReadOnlyList<DiscordAttachment> images)
        {
            Images ??= images ?? throw new ArgumentNullException(nameof(images));

            return this;
        }
    }
}