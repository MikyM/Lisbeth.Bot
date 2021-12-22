using Imgur.API;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlImage : IAsyncHtmlBuilder
{
    public HtmlImage(string discordLink, BotOptions options)
    {
        DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
        BotOptions ??= options ?? throw new ArgumentNullException(nameof(options));
    }

    public string DiscordLink { get; }
    public static List<string> SupportedTypes { get; } = new() { "png", "bmp", "jpg", "jpeg", "gif", "tif" };
    public BotOptions BotOptions { get; }

    public async Task<string> GetImgurLinkAsync()
    {
        if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last().Split('?').First())) return "";

        ApiClient apiClient = new (BotOptions.ImgurApiKey);
        IImage imageUpload;
        using HttpClient httpClient = ChatExportHttpClientFactory.Build();
        using HttpClient imgurHttpClient = ChatExportHttpClientFactory.Build();
        using HttpRequestMessage req = new(HttpMethod.Get, DiscordLink);
        using HttpResponseMessage response = await httpClient.SendAsync(req);
        Stream stream = await response.Content.ReadAsStreamAsync();
        try
        {
            ImageEndpoint imageEndpoint = new(apiClient, imgurHttpClient);
            imageUpload = await imageEndpoint.UploadImageAsync(stream);
        }
        catch (Exception)
        {
            return "";
        }

        return imageUpload.Link;
    }

    public async Task<string> BuildAsync()
    {
        string link = await GetImgurLinkAsync();
        return link switch
        {
            "" => "",
            _ => $"<img class=\"image\" src=\"{link}\"/>"
        };
    }
}