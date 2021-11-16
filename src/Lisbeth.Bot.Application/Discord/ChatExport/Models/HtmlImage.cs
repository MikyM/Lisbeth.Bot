using Autofac.Core;
using Imgur.API;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using MikyM.Common.Domain;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlImage : IAsyncHtmlBuilder
{
    public HtmlImage(string discordLink)
    {
        DiscordLink ??= discordLink ?? throw new ArgumentNullException(nameof(discordLink));
    }

    public string DiscordLink { get; }
    public static List<string> SupportedTypes { get; } = new() { "png", "bmp", "jpg", "jpeg", "gif", "tif" };

    public async Task<string> GetImgurLinkAsync()
    {
        if (SupportedTypes.All(x => x != DiscordLink.Split('.').Last().Split('?').First())) return "";

        if (ContainerProvider.Container.TryGetHttpClientFactory(out var httpClientFactory))
            throw new DependencyResolutionException("Failed to resolve IHttpClientFactory in Chat Builder.");

        ApiClient apiClient = new (Environment.GetEnvironmentVariable("IMGUR_KEY"));
        IImage imageUpload;
        using HttpClient httpClient = httpClientFactory.CreateClient();
        using HttpClient imgurHttpClient = httpClientFactory.CreateClient();
        using HttpRequestMessage req = new(HttpMethod.Get, DiscordLink);
        using HttpResponseMessage response = await httpClient.SendAsync(req);
        Stream stream = await response.Content.ReadAsStreamAsync();
        try
        {
            ImageEndpoint imageEndpoint = new(apiClient, imgurHttpClient);
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
        string link = await GetImgurLinkAsync();
        return link switch
        {
            "" => "",
            _ => $"<img class=\"image\" src=\"{link}\"/>"
        };
    }
}