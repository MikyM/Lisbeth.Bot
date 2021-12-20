using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlUser : IAsyncHtmlBuilder
{
    public HtmlUser(DiscordUser user, BotOptions options)
    {
        User ??= user ?? throw new ArgumentNullException(nameof(user));
        BotOptions ??= options ?? throw new ArgumentNullException(nameof(options));
    }

    public DiscordUser User { get; }
    public BotOptions BotOptions { get; }

    public Task<string> BuildAsync()
    {
        return Task.FromResult($"<div class=\"user-info\">{User.Username}#{User.Discriminator} ID: {User.Id}</div>");
    }

    public async Task<string> BuildAvatar()
    {
        var avatar = new HtmlImage(User.AvatarUrl, BotOptions);
        var url = await avatar.GetImgurLinkAsync();

        return $"<img class=\"avatar\" src={url}/>";
    }
}