using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlReaction : IAsyncHtmlBuilder
{
    public HtmlReaction(DiscordEmoji emoji, int count)
    {
        Emoji = emoji;
        Count = count;
    }

    public DiscordEmoji Emoji { get; }
    public int Count { get; }

    public Task<string> BuildAsync()
    {
        return Task.FromResult(Emoji.Name.Length is 2 or 1
            ? $"<div class=\"reaction-wrapper\">{Emoji.Name} {Count}</div>"
            : $"<div class=\"reaction-wrapper\"><img class=\"reaction\" src=\"{Emoji.Url}\">{Count}</div>");
    }
}
