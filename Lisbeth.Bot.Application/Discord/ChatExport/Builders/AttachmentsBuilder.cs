using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class AttachmentsBuilder
    {
        public IReadOnlyList<DiscordAttachment> Attachments { get; set; }

        public AttachmentsBuilder(IReadOnlyList<DiscordAttachment> attachments)
        {
            this.Attachments = attachments;
        }

        public async Task<string> GetHtml()
        {
            if (this.Attachments.Count == 0)
            {
                return "";
            }
            List<DiscordAttachment> imageAttachments = this.Attachments.Where(a => Image.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
            List<DiscordAttachment> videoAttachments = this.Attachments.Where(a => Video.SupportedTypes.Any(x => x == a.Url.Split('.').Last())).ToList();
            string imagesHtml = "";
            string videosHtml = "";

            if (imageAttachments.Count != 0)
            {
                ImagesBuilder imagesBuilder = new ImagesBuilder(imageAttachments);
                imagesHtml = await imagesBuilder.GetHtml();
            }
            if (videoAttachments.Count != 0)
            {
                VideosBuilder videosBuilder = new VideosBuilder(videoAttachments);
                videosHtml = await videosBuilder.GetHtml();
            }
            return $"<div class=\"attachments\">{imagesHtml}{videosHtml}</div>";
        }
    }
}