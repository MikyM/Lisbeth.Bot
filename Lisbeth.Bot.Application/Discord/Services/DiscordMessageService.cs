
// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Services.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly IDiscordService _discord;
        private readonly IPruneService _pruneService;

        public DiscordMessageService(IDiscordService discord, IPruneService pruneService)
        {
            _pruneService = pruneService;
            _discord = discord;
        }


        public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, InteractionContext ctx = null, bool isSingleMessageDelete = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await PruneAsync(req, logChannelId, null, null);

            return await PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member, ctx.ResolvedUserMentions?[0], null, isSingleMessageDelete, ctx.InteractionId);
        }

        public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null, bool isSingleMessageDelete = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await PruneAsync(req, logChannelId, null, null);

            return await PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member, 
                ctx.TargetMessage.Author != null 
                    ? ctx.TargetMessage.Author
                    : ctx.TargetUser != null 
                        ? ctx.TargetUser 
                        : null,
                ctx.TargetMessage != null 
                    ? ctx.TargetMessage 
                    : null, isSingleMessageDelete, ctx.InteractionId);
        }

        public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, DiscordChannel channel = null, DiscordGuild guild = null,
            DiscordUser moderator = null, DiscordUser author = null, DiscordMessage message = null, bool isSingleMessageDelete = false, ulong idToSkip = 0)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (guild is null)
            {
                try
                {
                    if (req.GuildId != null) guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
                }
            }

            if (author is null)
            {
                try
                {
                    if (req.TargetAuthorId != null && guild is not null) author = await guild.GetMemberAsync(req.TargetAuthorId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.TargetAuthorId} isn't the guilds member.");
                }
            }

            if (moderator is null)
            {
                try
                {
                    if (req.ModeratorId != null)
                        moderator = await _discord.Client.GetUserAsync(req.ModeratorId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.ModeratorId} doesn't exist.");
                }
            }

            if (channel is null)
            {
                try
                {
                    if (req.ChannelId != null) channel = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Channel with Id: {req.ChannelId} doesn't exist.");
                }
            }
            
            if (message is null)
            {
                try
                {
                    if (req.MessageId != null && channel is not null) await channel.GetMessageAsync(req.MessageId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Message with Id: {req.MessageId} doesn't exist.");
                }
            }

            if (logChannelId != 0)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(logChannelId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Channel with Id: {logChannelId} doesn't exist.");
                }
            }

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Prune result | {moderator.GetFullUsername()}", null, moderator != null ? moderator.AvatarUrl : null);

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
                    messagesToDelete.AddRange(messages.Where(x => x.Author.Id == author.Id).OrderByDescending(x => x.Timestamp).Take(req.Count));
                    deletedMessagesCount = messagesToDelete.Count;
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                if (req.MessageId is null && channel is not null && req.TargetAuthorId is null)
                {
                    var messages = await channel.GetMessagesAsync(req.Count);
                    messagesToDelete.AddRange(messages);
                    if(idToSkip != 0)
                        messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == idToSkip);
                    deletedMessagesCount = messagesToDelete.Count;
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                if (req.MessageId is not null && channel is not null && req.TargetAuthorId is null)
                {
                    DiscordMessage lastMessage = message;
                    while (true)
                    {
                        await Task.Delay(300);
                        messagesToDelete.Clear();
                        messagesToDelete.AddRange(await channel.GetMessagesAfterAsync(lastMessage.Id));
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
                    DiscordMessage lastMessage = message;
                    while (true)
                    {
                        await Task.Delay(300);
                        messagesToDelete.Clear();
                        var tempMessages = await channel.GetMessagesAfterAsync(lastMessage.Id);
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
            embed.AddField("Moderator", moderator != null ? moderator.Mention : null, true);
            embed.AddField("Delete count", deletedMessagesCount.ToString(), true);
            embed.AddField("Channel", channel != null ? channel.Mention : null, true);


            if (req.TargetAuthorId is not null)
            {
                embed.AddField("Target author", author.Mention, true);
                embed.WithAuthor($"Prune result | {author.GetFullUsername()}", null, author != null ? author.AvatarUrl : null);
            }

            var res = await _pruneService.AddAsync(req, true);

            return embed;
        }
    }
}
