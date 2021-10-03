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
        public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }

        public AttachmentsHtmlBuilder(IReadOnlyList<DiscordAttachment> attachments)
        {
            Attachments ??= attachments ?? throw new ArgumentNullException(nameof(attachments));
        }

        public AttachmentsHtmlBuilder WithAttachments(IReadOnlyList<DiscordAttachment> attachments)
        {
            Attachments ??= attachments ?? throw new ArgumentNullException(nameof(attachments));

            return this;
        }

        public async Task<string> BuildAsync()
        {
            if (Attachments.Count == 0)
            {
                return "";
            }
            List<DiscordAttachment> imageAttachments = Attachments.Where(a => HtmlImage.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
            List<DiscordAttachment> videoAttachments = Attachments.Where(a => HtmlVideo.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
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
    }
}