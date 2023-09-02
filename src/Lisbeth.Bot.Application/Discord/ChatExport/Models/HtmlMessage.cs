using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers.Message;
using Lisbeth.Bot.Domain;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlMessage : IAsyncHtmlBuilder
{
    public HtmlMessage(DiscordMessage message, BotConfiguration configuration)
    {
        Msg ??= message ?? throw new ArgumentNullException(nameof(message));
        BotConfiguration ??= configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DiscordMessage Msg { get; }
    public BotConfiguration BotConfiguration { get; private set; }

    public async Task<string> BuildAsync()
    {
        var messageBot = "";
        var attachmentsHtml = "";
        var reactionsHtml = "";

        var messageTop =
            $"<span class=\"nickname\">{Msg.Author?.Username ?? "Deleted user"}</span> <span class=\"message-info-details\">{Msg.Timestamp} Message ID: {Msg.Id}</span>";

        messageTop = $"<div class=\"message-info\">{messageTop}</div>";

        if (Msg.Content is not null && Msg.Content != "")
            messageBot = $"<div class=\"message-content\">{Msg.Content}</div>";

        if (Msg.Attachments.Count != 0)
        {
            AttachmentsHtmlWrapperBuilder attachmentsBuilder = new (Msg.Attachments, BotConfiguration);
            attachmentsHtml = await attachmentsBuilder.BuildAsync();
        }

        if (Msg.Reactions.Count != 0)
        {
            ReactionsHtmlWrapperBuilder reactionsHtmlBuilder = new (Msg.Reactions.ToList());
            reactionsHtml = await reactionsHtmlBuilder.BuildAsync();
        }

        return $"<div class=\"message\">{messageTop}{messageBot}{attachmentsHtml}{reactionsHtml}</div><hr>";
    }

    public HtmlMessage WithOptions(BotConfiguration configuration)
    {
        BotConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        return this;
    }
}
