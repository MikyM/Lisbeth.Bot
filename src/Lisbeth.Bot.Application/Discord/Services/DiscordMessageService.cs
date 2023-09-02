// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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
using System.Globalization;
using AutoMapper;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Prune;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;
using Microsoft.Extensions.Logging;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using NotFoundException = DSharpPlus.Exceptions.NotFoundException;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[ServiceImplementation<IDiscordMessageService>(ServiceLifetime.InstancePerLifetimeScope)]
public class DiscordMessageService : IDiscordMessageService
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly IPruneDataService _pruneDataService;
    private readonly ILogger<DiscordMessageService> _logger;
    private readonly IResponseDiscordEmbedBuilder<DiscordModeration> _embedBuilder;
    private readonly IDiscordGuildLoggerService _discordGuildLogger;
    private readonly IMapper _mapper;

    public DiscordMessageService(IDiscordService discord, IPruneDataService pruneDataService, IGuildDataService guildDataService,
        ILogger<DiscordMessageService> logger, IResponseDiscordEmbedBuilder<DiscordModeration> embedBuilder,
        IDiscordGuildLoggerService discordGuildLogger, IMapper mapper)
    {
        _pruneDataService = pruneDataService;
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _embedBuilder = embedBuilder;
        _discordGuildLogger = discordGuildLogger;
        _mapper = mapper;
    }


    public async Task<Result<DiscordEmbed>> PruneAsync(InteractionContext ctx, PruneReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await PruneAsync(req, ctx.Channel, ctx.Guild, ctx.Member, ctx.InteractionId);
    }

    public async Task<Result<DiscordEmbed>> PruneAsync(ContextMenuContext ctx, PruneReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await PruneAsync(req, ctx.Channel, ctx.Guild, ctx.Member, ctx.InteractionId);
    }

    public async Task<Result<DiscordEmbed>> PruneAsync(PruneReqDto req, DiscordChannel channel, DiscordGuild discordGuild,
        DiscordMember moderator, ulong? interactionId = null)
    {
        if (channel is null) throw new ArgumentNullException(nameof(channel));
        if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(discordGuild.Id));

        if (!guildRes.IsDefined(out var guild) || !guild.IsModerationModuleEnabled)
            return new NotFoundError("Guild not found or it does not have moderation enabled");

        DiscordUser? targetAuthor = null;
        if (req.TargetAuthorId.HasValue)
            targetAuthor = await _discord.Client.GetUserAsync(req.TargetAuthorId.Value);

        await _discordGuildLogger.LogToDiscordAsync(discordGuild, req, DiscordModeration.Prune, moderator, targetAuthor,
            guild.EmbedHexColor);

        var count = 0;

        if (req.Count is not null)
        {
            var messages = await channel.GetMessagesAsync(req.Count.Value);
            await channel.DeleteMessagesAsync(interactionId is null
                ? messages
                : messages.TakeWhile(x => x.Interaction is null || x.Interaction.Id != interactionId));
            count = messages.Count;
            req.Messages = _mapper.Map<List<MessageLog>>(messages);
        }
        else if (req.IsTargetedMessageDelete.HasValue && req.IsTargetedMessageDelete.Value && req.MessageId is not null)
        {
            DiscordMessage? targetMessage;

            try
            {
                targetMessage = await channel.GetMessageAsync(req.MessageId.Value);
            }
            catch (NotFoundException)
            {
                return new DiscordNotFoundError("Message with given Id was not found");
            }
            if (targetMessage is null)
                return new DiscordNotFoundError("Message with given Id was not found");

            await channel.DeleteMessageAsync(targetMessage);
            req.Messages = _mapper.Map<List<MessageLog>>(new List<DiscordMessage>{ targetMessage });
            count++;
        }
        else if (req.MessageId is not null)
        {
            List<DiscordMessage> messagesToDelete = new();
            var cycles = 0;
            var shouldStop = false;
            DiscordMessage? targetMessage;

            try
            {
                targetMessage = await channel.GetMessageAsync(req.MessageId.Value);
            }
            catch (NotFoundException)
            {
                return new DiscordNotFoundError("Message with given Id was not found");
            }
            if (targetMessage is null) 
                return new DiscordNotFoundError("Message with given Id was not found");

            var messages = await channel.GetMessagesAsync(1);
            var lastMessage = messages[0];

            while (cycles <= 10 && !shouldStop)
            {
                messagesToDelete.Clear();

                messagesToDelete.Add(lastMessage);

                messagesToDelete.AddRange(await channel.GetMessagesBeforeAsync(lastMessage.Id));
                await Task.Delay(300);

                var target = messagesToDelete.FirstOrDefault(x => x.Id == req.MessageId);
                if (target is not null)
                {
                    var index = messagesToDelete.IndexOf(target);
                    messagesToDelete.RemoveRange(index + 1, messagesToDelete.Count - index - 1);
                    shouldStop = true;
                }

                lastMessage = messagesToDelete.Last(); // save last message

                messagesToDelete.RemoveAll(x => x.Id == lastMessage.Id); // dont delete message so we can grab messages before it and we dont need to re-fetch

                await Task.Delay(600);

                // keep interaction message
                if (interactionId.HasValue)
                    messagesToDelete.RemoveAll(
                        x => x.Interaction is not null && x.Interaction.Id == interactionId.Value);

                try
                {
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }
                catch (BadRequestException)
                {
                    return new InvalidOperationError("Can't batch delete messages older than 14 days");
                }

                count += messagesToDelete.Count;
                cycles++;
                req.Messages.AddRange(_mapper.Map<List<MessageLog>>(messagesToDelete));
                await Task.Delay(1000);
            }
        }
        else if (req.TargetAuthorId.HasValue)
        {
            var messages = await channel.GetMessagesAsync();
            var messagesToDelete = messages.Where(x => x.Author.Id == req.TargetAuthorId.Value);
            req.Messages = _mapper.Map<List<MessageLog>>(messagesToDelete);
            await channel.DeleteMessagesAsync(messagesToDelete);
        }

        var res = await _pruneDataService.AddAsync(req, true);
        if (!res.IsDefined(out var id)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder.WithType(DiscordModeration.Prune)
            .EnrichFrom(new PruneReqResponseEnricher(req, count))
            .WithEmbedColor(new DiscordColor(guild.EmbedHexColor))
            .WithCase(id)
            .Build();
    }

    public async Task LogMessageUpdatedEventAsync(MessageUpdateEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (args.Message?.Author is null || args.MessageBefore is null)
            return;

        if (args.Author.IsBot || args.MessageBefore.Content == args.Message.Content &&
            args.MessageBefore.Attachments.Count == args.Message.Attachments.Count) return;

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) 
            return;

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageUpdatedEventsLogChannelId is null) return;

        var logChannel = args.Guild.GetChannel(guild.ModerationConfig.MessageUpdatedEventsLogChannelId);

        if (logChannel is null) return;

        var oldContent = args.MessageBefore.Content;
        var newContent = args.Message.Content;
        var oldAttachmentsString = "No attachments";
        var newAttachmentsString = "No attachments";

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
        embed.WithTitle("Message has been edited");
        embed.WithThumbnail(args.Author.AvatarUrl);
        
        embed.AddField("Author's identity", $"{args.Author.GetFullUsername()}", true);
        embed.AddField("Author's mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Author's ID and profile", $"[{args.Message.Author.Id}](https://discordapp.com/users/{args.Message.Author.Id})", true);
        
        embed.AddField("Channel's mention", $"{args.Channel.Mention}", true);
        embed.AddField("Channel's ID", $"{args.Channel.Id}", true);
        embed.AddField("Message's ID and link", $"[{args.Message.Id}](https://discordapp.com/channels/{args.Guild.Id}/{args.Channel.Id}/{args.Message.Id})", true);
        
        embed.AddField("Date sent", $"{args.Message.Timestamp.ToString(CultureInfo.CurrentCulture)}");
        embed.AddField("Old content", oldContent);
        embed.AddField("Old attachments", oldAttachmentsString);
        embed.AddField("New content", newContent);
        embed.AddField("New attachments", newAttachmentsString);
        embed.WithFooter($"Message Id: {args.Message.Id} | Author Id: {args.Message.Author.Id}");
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

        if (args.Message?.Author is null)
            return;

        if (args.Message.Author.IsBot) return;

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) return;

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        var logChannel = args.Guild.GetChannel(guild.ModerationConfig.MessageDeletedEventsLogChannelId);

        if (logChannel is null) return;

        var content = args.Message.Content;
        var attachmentsString = "No attachments";
        var attachments = args.Message.Attachments;
        var deletedBy = args.Message.Author;

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
        
        embed.AddField("Author's identity", $"{args.Message.Author.GetFullUsername()}", true);
        embed.AddField("Author's mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Author's ID and profile", $"[{args.Message.Author.Id}](https://discordapp.com/users/{args.Message.Author.Id})", true);
        
        embed.AddField("Channel's mention", $"{args.Channel.Mention}", true);
        embed.AddField("Channel's ID", $"{args.Channel.Id}", true);
        embed.AddField("Message's ID and link", $"[{args.Message.Id}](https://discordapp.com/channels/{args.Guild.Id}/{args.Channel.Id}/{args.Message.Id})", true);

        if (filteredBans.Count() != 0)
        {
            embed.WithTitle("Message has been deleted due to ban prune");
            embed.AddField("Deleted by", $"{filteredBans[0].UserResponsible.Mention}");
        }
        else
        {
            embed.WithTitle("Message has been deleted");
            embed.AddField("Deleted by", $"{deletedBy.Mention}");
        }

        embed.AddField("Date sent", $"{args.Message.Timestamp.ToString(CultureInfo.CurrentCulture)}");
        embed.AddField("Content", content);
        embed.AddField("Attachments", attachmentsString);
        embed.WithFooter($"Message Id: {args.Message.Id} | Author Id: {args.Message.Author.Id}");
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

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) return;

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        var logChannel = args.Guild.GetChannel(guild.ModerationConfig.MessageDeletedEventsLogChannelId);

        if (logChannel is null) return;

        await Task.Delay(500);

        var auditLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        var auditBulkLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MessageBulkDelete);
        var filtered = auditLogs
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .ToList();
        var filteredBulk = auditBulkLogs
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
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
            
            embed.AddField("Author's identity", $"{msg.Author.GetFullUsername()}", true);
            embed.AddField("Author's mention", $"{msg.Author.Mention}", true);
            embed.AddField("Author's ID and profile", $"[{msg.Author.Id}](https://discordapp.com/users/{msg.Author.Id})", true);
            
            embed.AddField("Channel's mention", $"{args.Channel.Mention}", true);
            embed.AddField("Channel's ID", $"{args.Channel.Id}", true);
            embed.AddField("Message's ID and link", $"[{msg.Id}](https://discordapp.com/channels/{args.Guild.Id}/{args.Channel.Id}/{msg.Id})", true);

            if (filtered.Count != 0)
            {
                embed.WithTitle("Message has been deleted due to ban prune");
                embed.AddField("Pruned by", $"{filtered[0].UserResponsible.Mention}");
            }
            else if (filteredBulk.Count() != 0)
            {
                embed.WithTitle("Message has been deleted via prune command");
                embed.AddField("Pruned by", $"{filteredBulk[0].UserResponsible.Mention}");
            }
            else
            {
                embed.WithTitle("Message has been deleted in a bulk deletion action");
                embed.AddField("Pruned by", "Unknown");
            }

            embed.AddField("Date sent", $"{msg.Timestamp.ToString(CultureInfo.CurrentCulture)}");
            embed.AddField("Content", content);
            embed.AddField("Attachments", attachmentsString);
            embed.WithFooter($"Message ID: {msg.Id} | Author ID: {msg.Author.Id}");
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
