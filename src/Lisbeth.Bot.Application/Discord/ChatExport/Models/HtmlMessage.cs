using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Models;

public class HtmlMessage
{
    public HtmlMessage(DiscordMessage message)
    {
        Msg ??= message ?? throw new ArgumentNullException(nameof(message));
    }

    public DiscordMessage Msg { get; }

    public async Task<string> Build()
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
            AttachmentsHtmlBuilder attachmentsBuilder = new AttachmentsHtmlBuilder(Msg.Attachments);
            attachmentsHtml = await attachmentsBuilder.BuildAsync();
        }

        if (Msg.Reactions.Count != 0)
        {
            ReactionsHtmlBuilder reactionsHtmlBuilder = new ReactionsHtmlBuilder(Msg.Reactions.ToList());
            reactionsHtml = await reactionsHtmlBuilder.BuildAsync();
        }

        return $"<div class=\"message\">{messageTop}{messageBot}{attachmentsHtml}{reactionsHtml}</div><hr>";
    }
}