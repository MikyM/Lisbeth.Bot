using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class VideosHtmlBuilder : IAsyncHtmlBuilder
    {
        public IReadOnlyList<DiscordAttachment> Videos { get; private set; }

        public VideosHtmlBuilder(IReadOnlyList<DiscordAttachment> videos)
        {
            Videos ??= videos ?? throw new ArgumentNullException(nameof(videos));
        }

        public VideosHtmlBuilder WithVideos(IReadOnlyList<DiscordAttachment> videos)
        {
            Videos ??= videos ?? throw new ArgumentNullException(nameof(videos));

            return this;
        }

        public async Task<string> BuildAsync()
        {
            if (Videos.Count == 0 || Videos == null)
            {
                return "";
            }
            string videosHtml = "";
            foreach (var attachment in Videos)
            {
                HtmlVideo video = new HtmlVideo(attachment.Url);
                videosHtml += await video.BuildAsync();
            }
            return $"<div class=\"videos-wrapper\">{videosHtml}</div>";
        }
    }
}