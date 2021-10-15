// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class AttachmentsHtmlBuilder : IAsyncHtmlBuilder
    {
        public AttachmentsHtmlBuilder(IReadOnlyList<DiscordAttachment> attachments)
        {
            Attachments ??= attachments ?? throw new ArgumentNullException(nameof(attachments));
        }

        public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }

        public async Task<string> BuildAsync()
        {
            if (Attachments.Count == 0) return "";
            List<DiscordAttachment> imageAttachments = Attachments
                .Where(a => HtmlImage.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
            List<DiscordAttachment> videoAttachments = Attachments
                .Where(a => HtmlVideo.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
            string imagesHtml = "";
            string videosHtml = "";

            if (imageAttachments.Count != 0)
            {
                ImagesHtmlBuilder imagesBuilder = new ImagesHtmlBuilder(imageAttachments);
                imagesHtml = await imagesBuilder.BuildAsync();
            }

            if (videoAttachments.Count != 0)
            {
                VideosHtmlBuilder videosBuilder = new VideosHtmlBuilder(videoAttachments);
                videosHtml = await videosBuilder.BuildAsync();
            }

            return $"<div class=\"attachments\">{imagesHtml}{videosHtml}</div>";
        }

        public AttachmentsHtmlBuilder WithAttachments(IReadOnlyList<DiscordAttachment> attachments)
        {
            Attachments ??= attachments ?? throw new ArgumentNullException(nameof(attachments));

            return this;
        }
    }
}