using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class ImagesHtmlBuilder
    {
        public IReadOnlyList<DiscordAttachment> Images { get; private set; }

        public ImagesHtmlBuilder(IReadOnlyList<DiscordAttachment> images)
        {
            Images ??= images ?? throw new ArgumentNullException(nameof(images));
        }

        public ImagesHtmlBuilder WithImages(IReadOnlyList<DiscordAttachment> images)
        {
            Images ??= images ?? throw new ArgumentNullException(nameof(images));

            return this;
        }
        public async Task<string> BuildAsync()
        {
            if (Images.Count == 0 || Images == null)
            {
                return "";
            }

            string imagesHtml = "";
            foreach (var attachment in Images)
            {
                HtmlImage image = new HtmlImage(attachment.Url);
                imagesHtml += await image.BuildAsync();
            }
            return $"<div class=\"images-wrapper\">{imagesHtml}</div>";
        }
    }
}