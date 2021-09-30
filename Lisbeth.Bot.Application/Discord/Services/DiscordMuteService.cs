using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<DiscordEmbed> MuteAsync(MuteReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild;
            DiscordMember member;
            DiscordUser moderator;
            DiscordChannel channel = null;

            if (ctx is null)
            {
                try
                {
                    await _discord.Client.GetUserAsync(req.UserId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
                }
                try
                {
                    guild = await _discord.Client.GetGuildAsync(req.GuildId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.GuildId} doesn't exist.");
                }
                try
                {
                    moderator = await _discord.Client.GetUserAsync(req.MutedById);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.MutedById} doesn't exist.");
                }
            }
            else
            {
                guild = ctx.Guild;
                moderator = ctx.Member;
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

            try
            {
                member = await guild.GetMemberAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Member with Id: {req.UserId} isn't the guilds member.");
            }

            if (req.MutedUntil is null) throw new ArgumentException($"{nameof(req.MutedUntil)} date was null");

            TimeSpan tmspDuration = req.MutedUntil.Value.Subtract(DateTime.UtcNow);

            string lengthString = req.MutedUntil.Value == DateTime.MaxValue ? "Permanent" : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            var res = await _muteService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"MuteAsync | {member.GetFullDisplayName()}", null, member.AvatarUrl);

            bool isMuted = member.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;
            bool resMute = true;

            if (res.FoundEntity is null)
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
                    embed.AddField("User mention", member.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Muted until", req.MutedUntil.ToString(), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {res.Id} | Member ID: {member.Id}");
                }
            }
            else
            {
                DiscordUser previousMod;
                try
                {
                    previousMod = await _discord.Client.GetUserAsync(res.FoundEntity.MutedById);
                }
                catch (Exception)
                {
                    previousMod = null;
                }

                if (res.FoundEntity.MutedUntil > req.MutedUntil)
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
                        embed.WithDescription($"This user has already been muted until {res.FoundEntity.MutedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                        embed.WithFooter($"Case ID: {res.Id} | Member ID: {res.FoundEntity.UserId}");
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
                        embed.AddField("Previous mute until", res.FoundEntity.MutedUntil.ToString(), true);
                        embed.AddField("Previous moderator", $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                        embed.AddField("Previous reason", res.FoundEntity.Reason, true);
                        embed.AddField("User mention", member.Mention, true);
                        embed.AddField("Moderator", moderator.Mention, true);
                        embed.AddField("Length", lengthString, true);
                        embed.AddField("Muted until", req.MutedUntil.ToString(), true);
                        embed.AddField("Reason", req.Reason);
                        embed.WithFooter($"Case ID: {res.Id} | Member ID: {member.Id}");
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

        public async Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild;
            DiscordMember member;
            DiscordUser moderator;
            DiscordChannel channel = null;

            try
            {
                await _discord.Client.GetUserAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
            }

            if (ctx is null)
            {
                try
                {
                    guild = await _discord.Client.GetGuildAsync(req.GuildId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.LiftedById} doesn't exist.");
                }
                try
                {
                    moderator = await _discord.Client.GetUserAsync(req.LiftedById);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.LiftedById} doesn't exist.");
                }
            }
            else
            {
                guild = ctx.Guild;
                moderator = ctx.Member;
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

            try
            {
                member = await guild.GetMemberAsync(req.UserId);
            }
            catch
            {
                throw new ArgumentException($"Member with Id: {req.UserId} isn't the guilds member.");
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
    }
}
