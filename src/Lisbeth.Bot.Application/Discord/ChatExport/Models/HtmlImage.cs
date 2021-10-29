using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Imgur.API;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using MikyM.Common.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlImage
    {
        public HtmlImage(string discordLink)
        {
            DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
        }

        public string DiscordLink { get; }
        public static List<string> SupportedTypes { get; } = new() {"png", "bmp", "jpg", "jpeg", "gif", "tif"};

        public async Task<string> GetImgurLink()
        {
            if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last().Split('?').First())) return "";

            var httpClientFactory = ContainerProvider.Container.Resolve<IHttpClientFactory>();

            ApiClient apiClient = new ApiClient(Environment.GetEnvironmentVariable("IMGUR_KEY"));
            IImage imageUpload;
            using HttpClient httpClient = httpClientFactory.CreateClient();
            using HttpClient imgurHttpClient = httpClientFactory.CreateClient();
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, DiscordLink);
            using HttpResponseMessage response = await httpClient.SendAsync(req);
            Stream stream = await response.Content.ReadAsStreamAsync();
            try
            {
                ImageEndpoint imageEndpoint = new ImageEndpoint(apiClient, imgurHttpClient);
                imageUpload = await imageEndpoint.UploadImageAsync(stream);
            }
            catch (ImgurException)
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