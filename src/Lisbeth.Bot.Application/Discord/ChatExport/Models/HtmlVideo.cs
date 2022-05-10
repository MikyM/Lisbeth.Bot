using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Domain;
using VimeoDotNet;
using VimeoDotNet.Net;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlVideo : IAsyncHtmlBuilder
{
    public HtmlVideo(string discordLink, BotConfiguration configuration)
    {
        DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private string DiscordLink { get; }
    public static List<string> SupportedTypes { get; } = new() { "mp4", "mov", "wmv", "avi", "flv" };
    public BotConfiguration BotConfiguration { get; }

    public async Task<string> GetVimeoLinkAsync()
    {
        if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last())) return "";

        var client = new VimeoClient(BotConfiguration.VimeoApiKey);
        IUploadRequest request;

        using (HttpClient httpClient = ChatExportHttpClientFactory.Build())
        {
            try
            {
                using HttpRequestMessage req = new(HttpMethod.Get, DiscordLink);
                using HttpResponseMessage response = await httpClient.SendAsync(req);
                Stream stream = await response.Content.ReadAsStreamAsync();
                request = await client.UploadEntireFileAsync(new BinaryContent(stream,
                    "application/x-www-form-urlencoded"));
            }
            catch (Exception)
            {
                return "";
            }
        }

        return $"https://player.vimeo.com/video/{request.ClipUri.Split('/').Last()}";
    }

    public async Task<string> BuildAsync()
    {
        string link = await GetVimeoLinkAsync();
        return link switch
        {
            "" => "",
            _ =>
                $"<div class=\"video\"><iframe src=\"{link}\" width=\"400\" height=\"240\" webkitallowfullscreen=\"\" mozallowfullscreen=\"\" allowfullscreen=\"\"></iframe></div>"
        };
    }
}