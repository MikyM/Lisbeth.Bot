using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Domain;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlUser : IAsyncHtmlBuilder
{
    public HtmlUser(DiscordUser user, BotConfiguration configuration)
    {
        User ??= user ?? throw new ArgumentNullException(nameof(user));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DiscordUser User { get; }
    public BotConfiguration BotConfiguration { get; }

    public Task<string> BuildAsync()
    {
        return Task.FromResult($"<div class=\"user-info\">{User.GetFullUsername()} ID: {User.Id}</div>");
    }

    public async Task<string> BuildAvatar()
    {
        var avatar = new HtmlImage(User.GetAvatarUrl(ImageFormat.Png), BotConfiguration);
        var url = await avatar.GetImgurLinkAsync();

        return $"<img class=\"avatar\" src={url}/>";
    }
}
