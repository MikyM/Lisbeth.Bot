// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordMessageService : IDiscordMessageService
{
    private readonly IDiscordService _discord;
    private readonly IGuildService _guildService;
    private readonly IPruneService _pruneService;

    public DiscordMessageService(IDiscordService discord, IPruneService pruneService, IGuildService guildService)
    {
        _pruneService = pruneService;
        _discord = discord;
        _guildService = guildService;
    }


    public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0,
        InteractionContext? ctx = null, bool isSingleMessageDelete = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (ctx is null) return await PruneAsync(req, logChannelId, null, null);

        return await PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member, ctx.ResolvedUserMentions[0],
            null, isSingleMessageDelete, ctx.InteractionId);
    }

    public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0,
        ContextMenuContext? ctx = null, bool isSingleMessageDelete = false)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (ctx is null) return await PruneAsync(req, logChannelId, null, null);

        return await PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member,
            ctx.TargetMessage.Author ?? ctx.TargetUser,
            ctx.TargetMessage, isSingleMessageDelete, ctx.InteractionId);
    }

    public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0,
        DiscordChannel? channel = null, DiscordGuild? guild = null,
        DiscordUser? moderator = null, DiscordUser? author = null, DiscordMessage? message = null,
        bool isSingleMessageDelete = false, ulong idToSkip = 0)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (guild is null)
            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId ?? throw new InvalidOperationException());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

        if (author is null)
            try
            {
                author = await guild.GetMemberAsync(req.TargetAuthorId ?? throw new InvalidOperationException());
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetAuthorId} isn't the guilds member.");
            }

        if (moderator is null)
            try
            {
                moderator = await _discord.Client.GetUserAsync(req.RequestedOnBehalfOfId ??
                                                               throw new InvalidOperationException());
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist.");
            }

        if (channel is null)
            try
            {
                if (req.ChannelId is not null) channel = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Channel with Id: {req.ChannelId} doesn't exist.");
            }

        if (message is null)
            try
            {
                if (req.MessageId is not null && channel is not null)
                    await channel.GetMessageAsync(req.MessageId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"message with Id: {req.MessageId} doesn't exist.");
            }

        if (logChannelId != 0)
            try
            {
                channel = await _discord.Client.GetChannelAsync(logChannelId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Channel with Id: {logChannelId} doesn't exist.");
            }

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(0x18315C);
        embed.WithAuthor($"Prune result | {moderator.GetFullUsername()}", null, moderator.AvatarUrl);

        int deletedMessagesCount = 0;

        List<DiscordMessage> messagesToDelete = new();


        if (isSingleMessageDelete)
        {
            if (channel is not null) await channel.DeleteMessageAsync(message);
            deletedMessagesCount++;
        }
        else
        {
            if (req.MessageId is null && channel is not null && req.TargetAuthorId is not null)
            {
                var messages = await channel.GetMessagesAsync();
                messagesToDelete.AddRange(messages.Where(x => x.Author.Id == author.Id)
                    .OrderByDescending(x => x.Timestamp).Take(req.Count));
                deletedMessagesCount = messagesToDelete.Count;
                await channel.DeleteMessagesAsync(messagesToDelete);
            }

            if (req.MessageId is null && channel is not null && req.TargetAuthorId is null)
            {
                var messages = await channel.GetMessagesAsync(req.Count);
                messagesToDelete.AddRange(messages);
                if (idToSkip != 0)
                    messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == idToSkip);
                deletedMessagesCount = messagesToDelete.Count;
                await channel.DeleteMessagesAsync(messagesToDelete);
            }

            if (req.MessageId is not null && channel is not null && req.TargetAuthorId is null)
            {
                DiscordMessage? lastMessage = message;
                while (true)
                {
                    await Task.Delay(300);
                    messagesToDelete.Clear();
                    messagesToDelete.AddRange(
                        await channel.GetMessagesAfterAsync(
                            lastMessage?.Id ?? throw new InvalidOperationException()));
                    if (idToSkip != 0)
                        messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == idToSkip);
                    if (messagesToDelete.Count == 0)
                        break;
                    deletedMessagesCount += messagesToDelete.Count;
                    lastMessage = messagesToDelete.Last();
                    await Task.Delay(200);
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                await channel.DeleteMessageAsync(message);
                deletedMessagesCount++;
            }

            if (req.MessageId is not null && channel is not null && req.TargetAuthorId is not null)
            {
                DiscordMessage? lastMessage = message;
                while (true)
                {
                    await Task.Delay(300);
                    messagesToDelete.Clear();
                    var tempMessages =
                        await channel.GetMessagesAfterAsync(
                            lastMessage?.Id ?? throw new InvalidOperationException());
                    if (messagesToDelete.Count == 0)
                        break;
                    messagesToDelete.AddRange(tempMessages.Where(x => x.Author.Id == author?.Id));
                    deletedMessagesCount += messagesToDelete.Count;
                    lastMessage = messagesToDelete.Last();
                    await Task.Delay(200);
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                await channel.DeleteMessageAsync(message);
                deletedMessagesCount++;
            }
        }

        embed.AddField("Moderator", moderator.Mention, true);
        embed.AddField("Delete count", deletedMessagesCount.ToString(), true);
        embed.AddField("Channel", channel?.Mention, true);


        if (req.TargetAuthorId is not null)
        {
            embed.AddField("Target author", author.Mention, true);
            embed.WithAuthor($"Prune result | {author.GetFullUsername()}", null, author.AvatarUrl);
        }

        _ = await _pruneService.AddAsync(req, true);

        return embed;
    }

    public async Task LogMessageUpdatedEventAsync(MessageUpdateEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (args.Author.IsBot || args.MessageBefore.Content == args.Message.Content &&
            args.MessageBefore.Attachments.Count == args.Message.Attachments.Count) return;

        var res = await _guildService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpecifications(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageUpdatedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageUpdatedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        string oldContent = args.MessageBefore.Content;
        string newContent = args.Message.Content;
        string oldAttachmentsString = "No attachments";
        string newAttachmentsString = "No attachments";

        var oldAttachments = args.MessageBefore.Attachments;
        var newAttachments = args.Message.Attachments;

        List<string> oldAttachmentUrls = oldAttachments.Select(attachment => attachment.ProxyUrl).ToList();
        List<string> newAttachmentUrls = newAttachments.Select(attachment => attachment.ProxyUrl).ToList();

        if (oldAttachmentUrls.Count != 0)
            oldAttachmentsString = string.Join(Environment.NewLine, oldAttachmentUrls);
        if (newAttachmentUrls.Count != 0)
            newAttachmentsString = string.Join(Environment.NewLine, newAttachmentUrls);

        if (oldContent == "") oldContent = "No content";
        if (newContent == "") newContent = "No content";

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("message has been edited");
        embed.WithThumbnail(args.Author.AvatarUrl);
        embed.AddField("Author", $"{args.Author.GetFullUsername()}", true);
        embed.AddField("Author mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Channel", $"{args.Channel.Mention}", true);
        embed.AddField("Date sent", $"{args.Message.Timestamp}");
        embed.AddField("Old content", oldContent);
        embed.AddField("Old attachments", oldAttachmentsString);
        embed.AddField("New content", newContent);
        embed.AddField("New attachments", newAttachmentsString);
        embed.WithFooter($"message Id: {args.Message.Id} || Author Id: {args.Message.Author.Id}");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));

        try
        {
            await _discord.Client.SendMessageAsync(logChannel, embed.Build());
        }
        catch (Exception)
        {
            //log something
        }
    }

    public async Task LogMessageDeletedEventAsync(MessageDeleteEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (args.Message.Author.IsBot) return;

        var res = await _guildService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpecifications(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageDeletedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        string content = args.Message.Content;
        string attachmentsString = "No attachments";
        var attachments = args.Message.Attachments;
        DiscordUser deletedBy = args.Message.Author;

        await Task.Delay(500);

        var auditLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MessageDelete);
        var auditLogsBans = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        var filtered = auditLogs
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();
        var filteredBans = auditLogsBans
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();

        if (filtered.Count() != 0)
        {
            var deletedLog = (DiscordAuditLogMessageEntry)filtered[0];
            if (deletedLog.Channel == args.Channel && args.Message.Author.Id == deletedLog.Target.Id)
                deletedBy = deletedLog.UserResponsible;
        }

        var attachmentUrls = attachments.Select(attachment => attachment.ProxyUrl).ToList();

        if (content == "") content = "No content";
        if (attachmentUrls.Count != 0) attachmentsString = string.Join(Environment.NewLine, attachmentUrls);

        var embed = new DiscordEmbedBuilder();

        embed.WithThumbnail(args.Message.Author.AvatarUrl);
        embed.AddField("Author", $"{args.Message.Author.GetFullUsername()}", true);
        embed.AddField("Author mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Channel", $"{args.Channel.Mention}", true);
        if (filteredBans.Count() != 0)
        {
            embed.WithTitle("message has been deleted due to ban prune");
            embed.AddField("Deleted by", $"{filteredBans[0].UserResponsible.Mention}");
        }
        else
        {
            embed.WithTitle("message has been deleted");
            embed.AddField("Deleted by", $"{deletedBy.Mention}");
        }

        embed.AddField("Date sent", $"{args.Message.Timestamp}");
        embed.AddField("Content", content);
        embed.AddField("Attachments", attachmentsString);
        embed.WithFooter($"message Id: {args.Message.Id} || Author Id: {args.Message.Author.Id}");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));

        try
        {
            await _discord.Client.SendMessageAsync(logChannel, embed.Build());
        }
        catch (Exception)
        {
            //log something
        }
    }

    public async Task LogMessageBulkDeletedEventAsync(MessageBulkDeleteEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        var res = await _guildService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpecifications(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageDeletedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        await Task.Delay(500);

        var auditLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        var auditBulkLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MessageBulkDelete);
        var filtered = auditLogs
            .Where(m => m.CreationTimestamp.LocalDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .ToList();
        var filteredBulk = auditBulkLogs
            .Where(m => m.CreationTimestamp.LocalDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .ToList();

        foreach (var msg in args.Messages)
        {
            if (msg?.Author is null || msg.Author.IsBot) continue;

            var embed = new DiscordEmbedBuilder();
            var attachments = msg.Attachments;
            var attachmentsString = "No attachments";

            var attachmentUrls = attachments.Select(attachment => attachment.ProxyUrl).ToList();

            if (attachmentUrls.Count != 0) attachmentsString = string.Join(Environment.NewLine, attachmentUrls);

            var content = msg.Content;
            if (content == "") content = "No content";

            embed.WithThumbnail(msg.Author.AvatarUrl);
            embed.AddField("Author", $"{msg.Author.Username}#{msg.Author.Discriminator}", true);
            embed.AddField("Author mention", $"{msg.Author.Mention}", true);
            embed.AddField("Channel", $"{args.Channel.Mention}", true);

            if (filtered.Count() != 0)
            {
                embed.WithTitle("message has been deleted due to ban prune");
                embed.AddField("Pruned by", $"{filtered[0].UserResponsible.Mention}");
            }
            else if (filteredBulk.Count() != 0)
            {
                embed.WithTitle("message has been deleted via prune command");
                embed.AddField("Pruned by", $"{filteredBulk[0].UserResponsible.Mention}");
            }
            else
            {
                embed.WithTitle("message has been deleted in a bulk deletion action");
                embed.AddField("Pruned by", "Unknown");
            }

            embed.AddField("Date sent", $"{msg.Timestamp}");
            embed.AddField("Content", content);
            embed.AddField("Attachments", attachmentsString);
            embed.WithFooter($"message ID: {msg.Id} || Author ID: {msg.Author.Id}");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));

            try
            {
                await _discord.Client.SendMessageAsync(logChannel, embed.Build());
            }
            catch (Exception)
            {
                //log something
            }

            await Task.Delay(800);
        }
    }
}