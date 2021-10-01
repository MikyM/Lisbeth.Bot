using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class VideosBuilder
    {
        private IReadOnlyList<DiscordAttachment> Attachments { get; set; }

        public VideosBuilder(IReadOnlyList<DiscordAttachment> attachments)
        {
            this.Attachments = attachments;
        }

        public async Task<string> GetHtml()
        {
            if (this.Attachments.Count == 0 || this.Attachments == null)
            {
                return "";
            }
            string videosHtml = "";
            foreach (var attachment in this.Attachments)
            {
                Video video = new Video(attachment.Url);
                videosHtml += await video.GetHtml();
            }
            return $"<div class=\"videos-wrapper\">{videosHtml}</div>";
        }
    }
}