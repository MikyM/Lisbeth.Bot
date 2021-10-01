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

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly IDiscordService _discord;
/*        private readonly IMessageService _messageService;*/

        public DiscordMessageService(IDiscordService discord/*, IMessageService messageService*/)
        {
/*            _messageService = messageService;*/
            _discord = discord;
        }


        public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, InteractionContext ctx = null, bool isSingleMessageDelete = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.PruneAsync(req, logChannelId, null, null);

            return await this.PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member, ctx.ResolvedUserMentions?[0], null, isSingleMessageDelete, ctx.InteractionId);
        }

        public async Task<DiscordEmbed> PruneAsync(PruneReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null, bool isSingleMessageDelete = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.PruneAsync(req, logChannelId, null, null);

            return await this.PruneAsync(req, logChannelId, ctx.Channel, ctx.Guild, ctx.Member, 
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
                    if (req.UserId != null && guild is not null) author = await guild.GetMemberAsync(req.UserId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.UserId} isn't the guilds member.");
                }
            }

            if (moderator is null)
            {
                try
                {
                    if (req.RequestedById != null)
                        moderator = await _discord.Client.GetUserAsync(req.RequestedById.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.RequestedById} doesn't exist.");
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
                if (req.MessageId is null && channel is not null && req.UserId is not null)
                {
                    var messages = await channel.GetMessagesAsync();
                    messagesToDelete.AddRange(messages.Where(x => x.Author.Id == author.Id).OrderByDescending(x => x.Timestamp).Take(req.Count));
                    deletedMessagesCount = messagesToDelete.Count;
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                if (req.MessageId is null && channel is not null && req.UserId is null)
                {
                    var messages = await channel.GetMessagesAsync(req.Count);
                    messagesToDelete.AddRange(messages);
                    if(idToSkip != 0)
                        messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == idToSkip);
                    deletedMessagesCount = messagesToDelete.Count;
                    await channel.DeleteMessagesAsync(messagesToDelete);
                }

                if (req.MessageId is not null && channel is not null && req.UserId is null)
                {
                    DiscordMessage lastMessage = message;
                    while (!messagesToDelete.Contains(message))
                    {
                        messagesToDelete.Clear();
                        messagesToDelete.AddRange(await channel.GetMessagesAfterAsync(lastMessage.Id));
                        if (idToSkip != 0)
                            messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == idToSkip);
                        deletedMessagesCount += messagesToDelete.Count;
                        lastMessage = messagesToDelete.Last();
                        await Task.Delay(200);
                        await channel.DeleteMessagesAsync(messagesToDelete);
                        await Task.Delay(1000);
                    }
                }

                if (req.MessageId is not null && channel is not null && req.UserId is not null)
                {
                    DiscordMessage lastMessage = message;
                    while (!messagesToDelete.Contains(message))
                    {
                        messagesToDelete.Clear();
                        var tempMessages = await channel.GetMessagesAfterAsync(lastMessage.Id);
                        messagesToDelete.AddRange(tempMessages.Where(x => x.Author.Id == author?.Id));
                        deletedMessagesCount += messagesToDelete.Count;
                        lastMessage = messagesToDelete.Last();
                        await Task.Delay(200);
                        await channel.DeleteMessagesAsync(messagesToDelete);
                        await Task.Delay(1000);
                    }
                }
            }
            embed.AddField("Moderator", moderator != null ? moderator.Mention : null, true);
            embed.AddField("Delete count", deletedMessagesCount.ToString(), true);
            embed.AddField("Channel", channel != null ? channel.Mention : null, true);
            if (author is not null)
            {
                embed.AddField("Target author", author.Mention, true);
            }

            return embed;
        }
    }
}
