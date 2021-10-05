using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Lisbeth.Bot.Application.Helpers;
using VimeoDotNet;
using VimeoDotNet.Net;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models
{
    public class HtmlVideo
    {
        private string DiscordLink { get; set; }
        public static List<string> SupportedTypes { get; } = new List<string> { "mp4", "mov", "wmv", "avi", "flv" };

        public HtmlVideo(string discordLink)
        {
            DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
        }

        public async Task<string> GetVimeoLink()
        {
            if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last()))
            {
                return "";
            }

            var client = new VimeoClient(Environment.GetEnvironmentVariable("VIMEO_KEY"));
            IUploadRequest request;

            using (HttpClient httpClient = ContainerProvider.Container.Resolve<IHttpClientFactory>().CreateClient())
            {
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, DiscordLink);
                using HttpResponseMessage response = await httpClient.SendAsync(req).ConfigureAwait(false);
                Stream stream = await response.Content.ReadAsStreamAsync();
                request = await client.UploadEntireFileAsync(new BinaryContent(stream,
                    "application/x-www-form-urlencoded"));
            }

            return $"https://player.vimeo.com/video/{request.ClipUri.Split('/').Last()}";
        }

        public async Task<string> BuildAsync()
        {
            string link = await GetVimeoLink();
            return link switch
            {
                "" => "",
                _ =>
                    $"<div class=\"video\"><iframe src=\"{link}\" width=\"400\" height=\"240\" webkitallowfullscreen=\"\" mozallowfullscreen=\"\" allowfullscreen=\"\"></iframe></div>"
            };
        }
    }
}