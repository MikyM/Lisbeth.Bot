using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class Image
    {
        private string DiscordLink { get; set; }
        private string ImgurLink { get; set; } = "";
        public static List<string> SupportedTypes { get; } = new List<string> { "png", "bmp", "jpg", "jpeg", "gif", "tif" };

        public Image(string discordLink)
        {
            this.DiscordLink = discordLink;
        }

        public async Task<string> GetImgurLink()
        {
            if (!SupportedTypes.Any(x => x == this.DiscordLink.Split('.').Last().Split('?').First()))
            {
                return "";
            }
            ApiClient apiClient = new ApiClient(Environment.GetEnvironmentVariable("IMGUR_KEY"));
            IImage imageUpload = null;
            using HttpClient httpClient = HttpClientFactory.CreateClient();
            using HttpClient imgurHttpClient = HttpClientFactory.CreateClient();
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, this.DiscordLink);
            using HttpResponseMessage response = await httpClient.SendAsync(req).ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync();
            try
            {
                ImageEndpoint imageEndpoint = new ImageEndpoint(apiClient, imgurHttpClient);
                imageUpload = await imageEndpoint.UploadImageAsync(stream);
            }
            catch (Imgur.API.ImgurException)
            {
                this.ImgurLink = "";
                return "";
            }

            this.ImgurLink = imageUpload.Link;
            return imageUpload.Link;
        }

        public async Task<string> GetHtml()
        {
            string link = await GetImgurLink();
            switch (link)
            {
                case "":
                    return "";

                default:
                    return $"<img class=\"image\" src=\"{link}\"/>";
            }
        }
    }
}