using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Discord.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Services.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordMuteService : IDiscordMuteService
    {
        private readonly IDiscordService _discord;
        private readonly IMuteService _muteService;

        public DiscordMuteService(IDiscordService discord, IMuteService muteService)
        {
            _muteService = muteService;
            _discord = discord;
        }

        public async Task<DiscordEmbed> MuteAsync(MuteReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.MuteAsync(req, logChannelId, null, null);

            return await this.MuteAsync(req, logChannelId, ctx.Guild, 
                ctx.TargetMember != null 
                    ? ctx.TargetMember
                    : ctx.TargetMessage != null
                        ? (DiscordMember)ctx.TargetMessage.Author
                        : null, ctx.Member);
        }

        public async Task<DiscordEmbed> MuteAsync(MuteReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.MuteAsync(req, logChannelId, null, null);

            return await this.MuteAsync(req, logChannelId, ctx.Guild, (DiscordMember)ctx.ResolvedUserMentions[0], ctx.Member);
        }

        public async Task<DiscordEmbed> MuteAsync(MuteReqDto req, ulong logChannelId = 0, DiscordGuild guild = null, DiscordMember member = null, DiscordUser moderator = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel channel = null;

            if (guild is null)
            {
                try
                {
                    guild = await _discord.Client.GetGuildAsync(req.GuildId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
                }
            }

            if (member is null)
            {
                try
                {
                    member = await guild.GetMemberAsync(req.UserId);
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
                    moderator = await _discord.Client.GetUserAsync(req.AppliedById);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.AppliedById} doesn't exist.");
                }
            }

            try
            {
                await _discord.Client.GetUserAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
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

            if (req.AppliedUntil is null) throw new ArgumentException($"{nameof(req.AppliedUntil)} date was null");

            TimeSpan tmspDuration = req.AppliedUntil.Value.Subtract(DateTime.UtcNow);

            string lengthString = req.AppliedUntil.Value == DateTime.MaxValue ? "Permanent" : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            var (id, foundEntity) = await _muteService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Mute | {member.GetFullDisplayName()}", null, member.AvatarUrl);

            bool isMuted = member.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;
            bool resMute = true;

            if (foundEntity is null)
            {
                if (!isMuted)
                    resMute = await member.Mute(guild);

                if (!resMute)
                {
                    var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                    embed.WithColor(0x18315C);
                    embed.WithAuthor($"{noEntryEmoji} Mute denied");
                    embed.WithDescription("Can't mute other moderators.");
                }
                else
                {
                    embed.AddField("User mention", member.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Muted until", req.AppliedUntil.ToString(), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {id} | Member ID: {member.Id}");
                }
            }
            else
            {
                DiscordUser previousMod;
                try
                {
                    previousMod = await _discord.Client.GetUserAsync(foundEntity.AppliedById);
                }
                catch (Exception)
                {
                    previousMod = null;
                }

                if (foundEntity.AppliedUntil > req.AppliedUntil)
                {
                    if (!isMuted)
                        resMute = await member.Mute(guild);

                    if (!resMute)
                    {
                        var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                        embed.WithColor(0x18315C);
                        embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                        embed.WithDescription("Can't mute other moderators.");
                    }
                    else
                    {
                        embed.WithDescription($"This user has already been muted until {foundEntity.AppliedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                        embed.WithFooter($"Case ID: {id} | Member ID: {foundEntity.UserId}");
                    }
                }
                else
                {
                    if (!isMuted)
                        resMute = await member.Mute(guild);

                    if (!resMute)
                    {
                        var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                        embed.WithColor(0x18315C);
                        embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                        embed.WithDescription("Can't mute other moderators.");
                    }
                    else
                    {
                        embed.WithAuthor($"Extend Mute | {member.GetFullUsername()}", null, member.AvatarUrl);
                        embed.AddField("Previous mute until", foundEntity.AppliedUntil.ToString(), true);
                        embed.AddField("Previous moderator", $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                        embed.AddField("Previous reason", foundEntity.Reason, true);
                        embed.AddField("User mention", member.Mention, true);
                        embed.AddField("Moderator", moderator.Mention, true);
                        embed.AddField("Length", lengthString, true);
                        embed.AddField("Muted until", req.AppliedUntil.ToString(), true);
                        embed.AddField("Reason", req.Reason);
                        embed.WithFooter($"Case ID: {id} | Member ID: {member.Id}");
                    }
                }
            }

            if (logChannelId == 0) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {logChannelId}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.UnmuteAsync(req, logChannelId, null, null);

            return await this.UnmuteAsync(req, logChannelId, ctx.Guild, ctx.TargetMember, ctx.Member);
        }

        public async Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.UnmuteAsync(req, logChannelId, null, null);

            DiscordMember member;
            try
            {
                member = await ctx.Guild.GetMemberAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} isn't the guilds member.");
            }

            return await this.UnmuteAsync(req, logChannelId, ctx.Guild, member, ctx.Member);
        }

        public async Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req, ulong logChannelId = 0, DiscordGuild guild = null, DiscordMember member = null, DiscordUser moderator = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel channel = null;

            if (guild is null)
            {
                try
                {
                    guild = await _discord.Client.GetGuildAsync(req.GuildId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
                }
            }

            if (member is null)
            {
                try
                {
                    member = await guild.GetMemberAsync(req.UserId);
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
                    moderator = await _discord.Client.GetUserAsync(req.LiftedById);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.LiftedById} doesn't exist.");
                }
            }

            try
            {
                await _discord.Client.GetUserAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
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
            
            if (req.LiftedOn is null) throw new ArgumentException($"{nameof(req.LiftedOn)} date was null");

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            bool isMuted = member?.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;

            var res = await _muteService.DisableAsync(req);

            if (res is null)
            {
                if (isMuted)
                {
                    await member.Unmute(guild);
                    embed.WithAuthor($"Unmute | {member.GetFullUsername()}", null, member.AvatarUrl);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("User mention", member.Mention, true);
                    embed.WithDescription($"Successfully unmuted");
                    embed.WithFooter($"Case ID: unknown | Member ID: {member.Id}");
                }
                else
                {
                    embed.WithAuthor($"Unmute failed | {member.GetFullDisplayName()}", null, member?.AvatarUrl);
                    embed.WithDescription($"This user isn't currently muted.");
                    embed.WithFooter($"Case ID: unknown | Member ID: {member?.Id}");
                }
            }
            else
            {
                await _muteService.CommitAsync();

                if(isMuted)
                    await member.Unmute(guild);

                embed.WithAuthor($"Unmute | {member.GetFullDisplayName()}", null, member?.AvatarUrl);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("User mention", member?.Mention, true);
                embed.WithDescription($"Successfully unmuted");
                embed.WithFooter($"Case ID: {res.Id} | Member ID: {member?.Id}");
            }

            if (logChannelId == 0) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {logChannelId}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> GetAsync(MuteGetReqDto req, ulong logChannelId = 0, ContextMenuContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.GetAsync(req, logChannelId, null, null);

            DiscordGuild guild = ctx.Guild;
            DiscordMember member = ctx.TargetMember;
            DiscordUser moderator = ctx.Member;

            return await this.GetAsync(req, logChannelId, guild, member, moderator);
        }

        public async Task<DiscordEmbed> GetAsync(MuteGetReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (ctx is null) return await this.GetAsync(req, logChannelId, null, null);

            DiscordMember member = null;

            try
            {
                if (req.UserId != null) member = await ctx.Guild.GetMemberAsync(req.UserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} isn't the guilds member.");
            }

            return await this.GetAsync(req, logChannelId, ctx.Guild, member, ctx.Member);
        }
        public async Task<DiscordEmbed> GetAsync(MuteGetReqDto req, ulong logChannelId = 0, DiscordGuild guild = null, DiscordMember member = null, DiscordUser moderator = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var res = await _muteService.GetBySpecificationsAsync<Mute>(
                new MuteBaseGetSpecifications(req.Id, req.UserId, req.GuildId, req.AppliedById, req.LiftedOn, req.AppliedOn, req.LiftedById));

            var entity = res.FirstOrDefault();

            DiscordChannel channel = null;

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

            if (member is null)
            {
                try
                {
                    if (req.UserId != null && guild is not null) member = await guild.GetMemberAsync(req.UserId.Value);
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
                    if (req.AppliedById != null) moderator = await _discord.Client.GetUserAsync(req.AppliedById.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.AppliedById} doesn't exist.");
                }
            }

            try
            {
                if (req.UserId != null) await _discord.Client.GetUserAsync(req.UserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
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

            if (entity is not null)
            {
                embed.WithAuthor($"Mute Info | {member.GetFullDisplayName()}", null, member?.AvatarUrl);
                embed.AddField("User mention", member?.Mention, true);
                embed.AddField("Moderator", $"{(moderator is not null ? moderator.Mention : "Deleted user")}", true);
                embed.AddField("Muted until", entity.AppliedUntil.ToString(), true);
                embed.AddField("Reason", entity.Reason);
                if (entity.LiftedById != 0)
                {
                    DiscordUser liftingMod = null;
                    try
                    {
                        liftingMod = await _discord.Client.GetUserAsync(entity.LiftedById);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    embed.AddField("Was lifted by", $"{(liftingMod is not null ? liftingMod.Mention : "Deleted user")}");
                }
                embed.WithFooter($"Case ID: {entity.Id} | User ID: {member.Id}");
            }
            else
            {
                embed.WithDescription("No mute info found.");
                embed.WithFooter($"Case ID: unknown | User ID: {member?.Id}");
            }

            if (logChannelId == 0) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {logChannelId}.");
            }

            return embed;
        }
    }
}
