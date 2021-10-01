using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class Video
    {
        private string DiscordLink { get; set; }
        private string VimeoLink { get; set; } = "";
        public static List<string> SupportedTypes { get; } = new List<string> { "mp4", "mov", "wmv", "avi", "flv" };

        public Video(string discordLink)
        {
            this.DiscordLink = discordLink;
        }

        public async Task<string> GetVimeoLink()
        {
            if (!SupportedTypes.Any(x => x == this.DiscordLink.Split('.').Last()))
            {
                return "";
            }
            var client = new VimeoClient(Environment.GetEnvironmentVariable("VIMEO_KEY"));
            IUploadRequest request;

            using (HttpClient httpClient = HttpClientFactory.CreateClient())
            {
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, this.DiscordLink);
                using HttpResponseMessage response = await httpClient.SendAsync(req).ConfigureAwait(false);
                Stream stream = await response.Content.ReadAsStreamAsync();
                request = await client.UploadEntireFileAsync(new BinaryContent(stream, "application/x-www-form-urlencoded"));
            }
            return $"https://player.vimeo.com/video/{request.ClipUri.Split('/').Last()}";
        }

        public async Task<string> GetHtml()
        {
            string link = await GetVimeoLink();
            switch (link)
            {
                case "":
                    return "";

                default:
                    return $"<div class=\"video\"><iframe src=\"{link}\" width=\"400\" height=\"240\" webkitallowfullscreen=\"\" mozallowfullscreen=\"\" allowfullscreen=\"\"></iframe></div>";
            }
        }
    }
}