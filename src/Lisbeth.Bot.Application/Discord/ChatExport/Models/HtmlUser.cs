using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlUser : IAsyncHtmlBuilder
{
    public HtmlUser(DiscordUser user)
    {
        User ??= user ?? throw new ArgumentNullException(nameof(user));
    }

    public DiscordUser User { get; }

    public Task<string> BuildAsync()
    {
        return Task.FromResult($"<div class=\"user-info\">{User.Username}#{User.Discriminator} ID: {User.Id}</div>");
    }

    public async Task<string> BuildAvatar()
    {
        var avatar = new HtmlImage(User.AvatarUrl);
        var url = await avatar.GetImgurLinkAsync();

        return $"<img class=\"avatar\" src={url}/>";
    }
}