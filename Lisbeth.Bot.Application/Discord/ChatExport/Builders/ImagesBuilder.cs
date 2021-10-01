using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public class ImagesBuilder
    {
        private IReadOnlyList<DiscordAttachment> Attachments { get; set; }

        public ImagesBuilder(IReadOnlyList<DiscordAttachment> attachments)
        {
            this.Attachments = attachments;
        }

        public async Task<string> GetHtml()
        {
            if (this.Attachments.Count == 0 || this.Attachments == null)
            {
                return "";
            }
            string imagesHtml = "";
            foreach (var attachment in this.Attachments)
            {
                Image image = new Image(attachment.Url);
                imagesHtml += await image.GetHtml();
            }
            return $"<div class=\"images-wrapper\">{imagesHtml}</div>";
        }
    }
}