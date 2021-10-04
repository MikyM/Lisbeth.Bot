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
using Lisbeth.Bot.DataAccessLayer.Specifications.BanSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Discord.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Services.Interfaces;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordBanService : IDiscordBanService
    {
        private readonly IDiscordService _discord;
        private readonly IBanService _banService;
        private readonly IGuildService _guildService;

        public DiscordBanService(IDiscordService discord, IBanService banService, IGuildService guildService)
        {
            _banService = banService;
            _discord = discord;
            _guildService = guildService;
        }

        public async Task<DiscordEmbed> BanAsync(BanReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordMember moderator;
            DiscordGuild guild;

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's target.");
            }

            return await BanAsync(guild, target, moderator, req.AppliedUntil, req.Reason, req);
        }

        public async Task<DiscordEmbed> BanAsync(InteractionContext ctx, DateTime appliedUntil, string reason = "")
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await BanAsync(ctx.Guild, (DiscordMember)ctx.ResolvedUserMentions[0], ctx.Member, appliedUntil, reason);
        }

        private async Task<DiscordEmbed> BanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator, DateTime appliedUntil, string reason = "", BanReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            DiscordBan ban;
            DiscordChannel channel = null;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            if (appliedUntil < DateTime.UtcNow) throw new ArgumentException("Mute until date must be in the future.");

            try
            {
                ban = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                ban = null;
            }

            req ??= new BanReqDto(target.Id, guild.Id, moderator.Id, appliedUntil, reason);

            var (id, foundEntity) = await _banService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Ban | {target.GetFullUsername()}", null, target.AvatarUrl);

            TimeSpan tmspDuration = appliedUntil.Subtract(DateTime.UtcNow);
            
            string lengthString = appliedUntil == DateTime.MaxValue ? "Permanent" : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            if (foundEntity is null)
            {
                if(ban is null)
                    await guild.BanMemberAsync(target.Id);

                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("Length", lengthString, true);
                embed.AddField("Banned until", appliedUntil.ToString(CultureInfo.InvariantCulture), true);
                embed.AddField("Reason", reason);
                embed.WithFooter($"Case ID: {id} | User ID: {target.Id}");
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
                    embed.WithDescription($"This user has already been banned until {foundEntity.AppliedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                    embed.WithFooter($"Case ID: {id} | User ID: {target.Id}");
                }
                else
                {
                    if (ban is null)
                        await guild.BanMemberAsync(req.TargetUserId);

                    embed.WithAuthor($"Extend BanAsync | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.AddField("Previous ban until", foundEntity.AppliedUntil.ToString(), true);
                    embed.AddField("Previous moderator", $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                    embed.AddField("Previous reason", foundEntity.Reason, true);
                    embed.AddField("User mention", target.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Banned until", appliedUntil.ToString(CultureInfo.InvariantCulture), true);
                    embed.AddField("Reason", reason);
                    embed.WithFooter($"Case ID: {id} | User ID: {target.Id}");
                }
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {guildCfg.LogChannelId}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> UnbanAsync(BanDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordMember moderator;
            DiscordGuild guild;
            Ban ban;

            if (req.Id is null && (req.GuildId is null || req.TargetUserId is null))
                throw new ArgumentException("You must supply either ban Id or guild Id and user Id");

            if (req.Id is not null)
            {
                ban = await _banService.GetAsync<Ban>(req.Id.Value);
                if (ban is null) throw new ArgumentException("Ban not found");
                req.GuildId = ban.GuildId;
                req.TargetUserId = ban.UserId;
            }

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's target.");
            }

            return await UnbanAsync(guild, target, moderator);
        }

        public async Task<DiscordEmbed> UnbanAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await UnbanAsync(ctx.Guild, ctx.ResolvedUserMentions[0], ctx.Member);
        }

        private async Task<DiscordEmbed> UnbanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator, BanDisableReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            DiscordChannel channel = null;
            DiscordBan ban;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            try
            {
                ban = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                ban = null;
            }

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Unban | {target.GetFullUsername()}", null, target.AvatarUrl);

            var res = await _banService.DisableAsync(req);

            if (ban is null)
            {
                embed.WithFooter($"Case ID: unknown  | Member ID: {target.Id}");

                if (res is not null)
                {
                    embed.WithFooter($"Case ID: {res.Id}  | Member ID: {target.Id}");
                    await _banService.CommitAsync();
                }
                else
                {
                    embed.WithAuthor($"Unban failed | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.WithDescription($"This user isn't currently banned.");
                }
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                await target.UnbanAsync(guild);
                await _banService.CommitAsync();

                embed.WithAuthor($"UnbanAsync | {target.GetFullUsername()}", null, target.AvatarUrl);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("User mention", target.Mention, true);
                embed.WithDescription($"Successfully unbanned");
                embed.WithFooter($"Case ID: {res.Id} | Member ID: {target.Id}");
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {channel.Id}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> GetSpecificUserGuildBanAsync(BanGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordMember moderator;
            DiscordGuild guild;

            if (req.Id is null && (req.GuildId is null || req.TargetUserId is null))
                throw new ArgumentException("You must supply either ban Id or guild Id and user Id");

            if (req.Id is not null)
            {
                var ban = await _banService.GetAsync<Ban>(req.Id.Value);
                if (ban is null) throw new ArgumentException("Ban not found");
                req.GuildId = ban.GuildId;
                req.TargetUserId = ban.UserId;
                req.AppliedById = ban.AppliedById;
                req.LiftedById = ban.LiftedById;
                req.AppliedOn = ban.AppliedOn;
            }

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's target.");
            }

            return await GetAsync(guild, target, moderator);
        }

        public async Task<DiscordEmbed> GetAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await GetAsync(ctx.Guild, ctx.ResolvedUserMentions[0], ctx.Member);
        }

        private async Task<DiscordEmbed> GetAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator, BanGetReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            DiscordChannel channel = null;
            DiscordBan discordBan;

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            try
            {
                discordBan = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                discordBan = null;
            }


            req ??= new BanGetReqDto(moderator.Id, null, target.Id, guild.Id);

            var res = await _banService.GetBySpecificationsAsync<Ban>(
                new BanBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn, req.AppliedOn, req.LiftedById, 0));

            var ban = res.FirstOrDefault();

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            if (ban is not null)
            {
                DiscordUser banningMod = null;
                try
                {
                    banningMod = await _discord.Client.GetUserAsync(ban.AppliedById);
                }
                catch (Exception)
                {
                    // ignore
                }

                embed.WithAuthor($"Ban Info | {target.GetFullUsername()}", null, target.AvatarUrl);
                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", $"{(banningMod is not null ? banningMod.Mention : "Deleted user")}", true);
                embed.AddField("Banned until", ban.AppliedUntil.ToString(), true);
                embed.AddField("Reason", ban.Reason);
                if (ban.LiftedById != 0)
                {
                    DiscordUser liftingMod = null;
                    try
                    {
                        if (req.LiftedById != null) liftingMod = await _discord.Client.GetUserAsync(ban.LiftedById);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
 
                    embed.AddField("Was lifted by", $"{(liftingMod is not null ? liftingMod.Mention : "Deleted user")}");
                }
                embed.WithFooter($"Case ID: {ban.Id} | User ID: {target.Id}");
            }
            else
            {
                if (discordBan is not null)
                {
                    embed.WithAuthor($"Ban Info (from Discord) | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.AddField("User mention", target.Mention, true);
                    embed.AddField("Moderator", "Unknown", true);
                    embed.AddField("Banned until", "Permanently", true);
                    embed.AddField("Reason", discordBan.Reason);
                    embed.WithFooter($"Case ID: unknown | User ID: {target.Id}");
                }
                else
                {
                    embed.WithAuthor($"Ban Info | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.WithDescription("Ban info not found.");
                    embed.WithFooter($"Case ID: unknown | User ID: {target.Id}");
                }
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {guildCfg.LogChannelId}.");
            }

            return embed;
        }
    }
}