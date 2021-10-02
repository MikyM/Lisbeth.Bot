using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlImage
    {
        public string DiscordLink { get; private set; }
        public static List<string> SupportedTypes { get; } = new() { "png", "bmp", "jpg", "jpeg", "gif", "tif" };

        public HtmlImage(string discordLink)
        {
            DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
        }

        public async Task<string> GetImgurLink()
        {
            if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last().Split('?').First()))
            {
                return "";
            }
            ApiClient apiClient = new ApiClient(Environment.GetEnvironmentVariable("IMGUR_KEY"));
            IImage imageUpload;
            using HttpClient httpClient = HttpClientFactory.CreateClient();
            using HttpClient imgurHttpClient = HttpClientFactory.CreateClient();
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, DiscordLink);
            using HttpResponseMessage response = await httpClient.SendAsync(req);
            Stream stream = await response.Content.ReadAsStreamAsync();
            try
            {
                ImageEndpoint imageEndpoint = new ImageEndpoint(apiClient, imgurHttpClient);
                imageUpload = await imageEndpoint.UploadImageAsync(stream);
            }
            catch (Imgur.API.ImgurException)
            {
                return "";
            }

            return imageUpload.Link;
        }

        public async Task<string> BuildAsync()
        {
            string link = await GetImgurLink();
            return link switch
            {
                "" => "",
                _ => $"<img class=\"image\" src=\"{link}\"/>"
            };
        }
    }
}