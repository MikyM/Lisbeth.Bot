using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordBanService : IDiscordBanService
    {
        private readonly IDiscordService _discord;
        private readonly IBanService _banService;

        public DiscordBanService(IDiscordService discord, IBanService banService)
        {
            _banService = banService;
            _discord = discord;
        }

        public async Task<DiscordEmbed> BanAsync(BanReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser user;
            DiscordGuild guild;
            DiscordUser moderator;
            DiscordBan ban;
            DiscordChannel channel = null;

            if (ctx is null)
            {
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
                    moderator = await _discord.Client.GetUserAsync(req.BannedById);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.BannedById} doesn't exist.");
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
                user = await _discord.Client.GetUserAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
            }

            try
            {
                ban = await guild.GetBanAsync(req.UserId);
            }
            catch (Exception)
            {
                ban = null;
            }

            var (id, foundEntity) = await _banService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"BanAsync | {user?.GetFullUsername()}", null, user?.AvatarUrl);

            if (req.BannedUntil is null) throw new ArgumentException($"{nameof(req.BannedUntil)} date was null");

            TimeSpan tmspDuration = req.BannedUntil.Value.Subtract(DateTime.UtcNow);
            
            string lengthString = req.BannedUntil.Value == DateTime.MaxValue ? "Permanent" : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            if (foundEntity is null)
            {
                if(ban is null)
                    await guild.BanMemberAsync(req.UserId);

                embed.AddField("User mention", user?.Mention, true);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("Length", lengthString, true);
                embed.AddField("Banned until", req.BannedUntil.ToString(), true);
                embed.AddField("Reason", req.Reason);
                embed.WithFooter($"Case ID: {id} | User ID: {user?.Id}");
            }
            else
            {
                DiscordUser previousMod;
                try
                {
                    previousMod = await _discord.Client.GetUserAsync(foundEntity.BannedById);
                }
                catch (Exception)
                {
                    previousMod = null;
                }

                if (foundEntity.BannedUntil > req.BannedUntil)
                {
                    embed.WithDescription($"This user has already been banned until {foundEntity.BannedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                    embed.WithFooter($"Case ID: {id} | User ID: {user?.Id}");
                }
                else
                {
                    if (ban is null)
                        await guild.BanMemberAsync(req.UserId);

                    embed.WithAuthor($"Extend BanAsync | {user.GetFullUsername()}", null, user?.AvatarUrl);
                    embed.AddField("Previous ban until", foundEntity.BannedUntil.ToString(), true);
                    embed.AddField("Previous moderator", $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                    embed.AddField("Previous reason", foundEntity.Reason, true);
                    embed.AddField("User mention", user?.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Banned until", req.BannedUntil.ToString(), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {id} | User ID: {user?.Id}");
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

        public async Task<DiscordEmbed> UnbanAsync(BanDisableReqDto req, ulong logChannelId = 0, InteractionContext ctx = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser user;
            DiscordGuild guild;
            DiscordUser moderator;
            DiscordBan ban;
            DiscordChannel channel = null;

            if (ctx is null)
            {
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
                user = await _discord.Client.GetUserAsync(req.UserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.UserId} doesn't exist.");
            }

            try
            {
                ban = await guild.GetBanAsync(req.UserId);
            }
            catch (Exception)
            {
                ban = null;
            }

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"UnbanAsync | {user?.GetFullUsername()}", null, user?.AvatarUrl);

            var res = await _banService.DisableAsync(req);

            if (req.LiftedOn is null) throw new ArgumentException($"{nameof(req.LiftedOn)} date was null");

            if (ban is null)
            {
                embed.WithFooter($"Case ID: unknown  | Member ID: {user?.Id}");

                if (res is not null)
                {
                    embed.WithFooter($"Case ID: {res.Id}  | Member ID: {user?.Id}");
                    await _banService.CommitAsync();
                }
                else
                {
                    embed.WithAuthor($"UnbanAsync failed | {user?.GetFullUsername()}", null, user?.AvatarUrl);
                    embed.WithDescription($"This user isn't currently banned.");
                }
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                await user.UnbanAsync(guild);
                await _banService.CommitAsync();

                embed.WithAuthor($"UnbanAsync | {user.GetFullUsername()}", null, user.AvatarUrl);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("User mention", user.Mention, true);
                embed.WithDescription($"Successfully unbanned");
                embed.WithFooter($"Case ID: {res.Id} | Member ID: {user.Id}");
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
