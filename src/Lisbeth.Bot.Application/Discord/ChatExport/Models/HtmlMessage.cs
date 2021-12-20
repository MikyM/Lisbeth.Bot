using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlMessage : IAsyncHtmlBuilder
{
    public HtmlMessage(DiscordMessage message, BotOptions options)
    {
        Msg ??= message ?? throw new ArgumentNullException(nameof(message));
        BotOptions ??= options ?? throw new ArgumentNullException(nameof(options));
    }

    public DiscordMessage Msg { get; }
    public BotOptions BotOptions { get; private set; }

    public async Task<string> BuildAsync()
    {
        string messageBot = "";
        string attachmentsHtml = "";
        string reactionsHtml = "";

        var messageTop =
            $"<span class=\"nickname\">{Msg.Author.Username}</span> <span class=\"message-info-details\">{Msg.Timestamp} message ID: {Msg.Id}</span>";

        messageTop = $"<div class=\"message-info\">{messageTop}</div>";

        if (Msg.Content is not null && Msg.Content != "")
            messageBot = $"<div class=\"message-content\">{Msg.Content}</div>";

        if (Msg.Attachments.Count != 0)
        {
            AttachmentsHtmlWrapperBuilder attachmentsBuilder = new (Msg.Attachments, BotOptions);
            attachmentsHtml = await attachmentsBuilder.BuildAsync();
        }

        if (Msg.Reactions.Count != 0)
        {
            ReactionsHtmlWrapperBuilder reactionsHtmlBuilder = new (Msg.Reactions.ToList());
            reactionsHtml = await reactionsHtmlBuilder.BuildAsync();
        }

        return $"<div class=\"message\">{messageTop}{messageBot}{attachmentsHtml}{reactionsHtml}</div><hr>";
    }

    public HtmlMessage WithOptions(BotOptions options)
    {
        BotOptions = options ?? throw new ArgumentNullException(nameof(options));

        return this;
    }
}