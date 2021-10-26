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

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hangfire;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer.Specifications.BanSpecifications;
using Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordBanService : IDiscordBanService
    {
        private readonly IBanService _banService;
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly ILogger<DiscordBanService> _logger;

        public DiscordBanService(IBanService banService, IDiscordService discord, IGuildService guildService,
            ILogger<DiscordBanService> logger)
        {
            _banService = banService;
            _discord = discord;
            _guildService = guildService;
            _logger = logger;
        }

        [Queue("moderation")]
        public async Task UnbanCheckAsync()
        {
            try
            {
                var res = await _banService.GetBySpecificationsAsync<Mute>(
                    new ActiveExpiredBansInActiveGuildsSpecifications());

                if (res is null || res.Count == 0) return;

                foreach (var ban in res)
                {
                    var req = new BanDisableReqDto(ban.UserId, ban.GuildId, _discord.Client.CurrentUser.Id);
                    await UnbanAsync(req);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Automatic unban failed with: {ex.GetFullMessage()}");
            }
        }

        public async Task<DiscordEmbed> BanAsync(BanReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember target = await guild.GetMemberAsync(req.TargetUserId);
            DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await BanAsync(guild, target, moderator, req);
        }

        public async Task<DiscordEmbed> BanAsync(InteractionContext ctx, BanReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
            {
                target = ctx.ResolvedUserMentions[0];
            }
            else
            {
                try
                {
                    target = await _discord.Client.GetUserAsync(req.TargetUserId);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException($"User with Id: {req.TargetUserId} doesn't exist.", ex);
                }
            }

            return await BanAsync(ctx.Guild, target, ctx.Member, req);
        }

        private async Task<DiscordEmbed> BanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
            BanReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (target is null) throw new ArgumentNullException(nameof(target));

            if (moderator.Guild.Id != guild.Id) throw new ArgumentException(nameof(moderator));

            DiscordMember targetMember;
            try
            {
                targetMember = await guild.GetMemberAsync(target.Id);
            }
            catch (Exception)
            {
                targetMember = null;
            }

            if (!moderator.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException($"User with Id: {moderator.Id} doesn't have moderator rights");
            if (targetMember is not null && targetMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException(
                    $"User with Id: {moderator.Id} doesn't have rights to ban another moderator");

            DiscordBan ban;
            DiscordChannel channel = null;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (guildCfg.ModerationConfig is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is not null)
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId
                        .Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"Log channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId} doesn't exist.",
                        ex);
                }

            try
            {
                ban = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                ban = null;
            }

            var (id, foundEntity) = await _banService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Ban | {target.GetFullUsername()}", null, target.AvatarUrl);

            TimeSpan tmspDuration = req.AppliedUntil.Subtract(DateTime.UtcNow);

            string lengthString = req.AppliedUntil == DateTime.MaxValue
                ? "Permanent"
                : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            if (foundEntity is null)
            {
                if (ban is null)
                    await guild.BanMemberAsync(target.Id);

                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("Length", lengthString, true);
                embed.AddField("Banned until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                embed.AddField("Reason", req.Reason);
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
                    embed.WithDescription(
                        $"This user has already been banned until {foundEntity.AppliedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                    embed.WithFooter($"Case ID: {id} | User ID: {target.Id}");
                }
                else
                {
                    if (ban is null) await guild.BanMemberAsync(req.TargetUserId);

                    embed.WithAuthor($"Extend BanAsync | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.AddField("Previous ban until", foundEntity.AppliedUntil.ToString(), true);
                    embed.AddField("Previous moderator",
                        $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                    embed.AddField("Previous reason", foundEntity.Reason, true);
                    embed.AddField("User mention", target.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Banned until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {id} | User ID: {target.Id}");
                }
            }

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is null)
                return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Can't send messages in channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> UnbanAsync(BanDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild;
            DiscordMember target;

            if (req.Id.HasValue)
            {
                var ban = await _banService.GetAsync<Ban>(req.Id.Value);
                if (ban is null) throw new ArgumentException("Ban not found");
                req.GuildId = ban.GuildId;
                req.TargetUserId = ban.UserId;
            }

            if (req.TargetUserId.HasValue && req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            else
            {
                throw new InvalidOperationException();
            }

            DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await UnbanAsync(guild, target, moderator, req);
        }

        public async Task<DiscordEmbed> UnbanAsync(InteractionContext ctx, BanDisableReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
            {
                target = ctx.ResolvedUserMentions[0];
            }
            else
            {
                try
                {
                    if (req.TargetUserId is not null) target = await _discord.Client.GetUserAsync(req.TargetUserId.Value);
                    else
                    {
                        var res = await _banService.GetAsync<Ban>(req.Id.Value);

                        if (res is not null)
                        {
                            target = await _discord.Client.GetUserAsync(res.UserId);
                        }
                        else
                        {
                            throw new ArgumentException(nameof(req.Id));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException($"User with Id: {req.TargetUserId} doesn't exist.", ex);
                }
            }

            return await UnbanAsync(ctx.Guild, target, ctx.Member, req);
        }

        public async Task<DiscordEmbed> GetSpecificUserGuildBanAsync(BanGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordGuild guild;

            if (req.Id.HasValue)
            {
                var ban = await _banService.GetAsync<Ban>(req.Id.Value);
                if (ban is null) throw new ArgumentException("Ban not found");
                req.GuildId = ban.GuildId;
                req.TargetUserId = ban.UserId;
                req.AppliedById = ban.AppliedById;
                req.LiftedById = ban.LiftedById;
                req.AppliedOn = ban.CreatedAt;
            }

            if (req.TargetUserId.HasValue && req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            else
            {
                throw new InvalidOperationException();
            }

            DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await GetAsync(guild, target, moderator, req);
        }

        public async Task<DiscordEmbed> GetSpecificUserGuildBanAsync(InteractionContext ctx, BanGetReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
            {
                target = ctx.ResolvedUserMentions[0];
            }
            else
            {
                try
                {
                    if (req.TargetUserId is not null) target = await _discord.Client.GetUserAsync(req.TargetUserId.Value);
                    else
                    {
                        var res = await _banService.GetAsync<Ban>(req.Id.Value);

                        if (res is not null)
                        {
                            target = await _discord.Client.GetUserAsync(res.UserId);
                        }
                        else
                        {
                            throw new ArgumentException(nameof(req.Id));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException($"User with Id: {req.TargetUserId} doesn't exist.", ex);
                }
            }

            return await GetAsync(ctx.Guild, target, ctx.Member, req);
        }

        private async Task<DiscordEmbed> UnbanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
            BanDisableReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel channel = null;
            DiscordBan ban;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (guildCfg.ModerationConfig is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is not null)
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId
                        .Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"Log channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId} doesn't exist.",
                        ex);
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
                    embed.WithDescription("This user isn't currently banned.");
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
                embed.WithDescription("Successfully unbanned");
                embed.WithFooter($"Case ID: {res.Id} | Member ID: {target.Id}");
            }

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is null)
                return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Can't send messages in channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId}.");
            }

            return embed;
        }

        private async Task<DiscordEmbed> GetAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
            BanGetReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (guildCfg.ModerationConfig is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            DiscordChannel channel = null;
            DiscordBan discordBan;

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is not null)
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId
                        .Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"Log channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId} doesn't exist.",
                        ex);
                }

            try
            {
                discordBan = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                discordBan = null;
            }

            var res = await _banService.GetBySpecificationsAsync<Ban>(
                new BanBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn,
                    req.AppliedOn, req.LiftedById));

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
                        if (req.LiftedById is not null) liftingMod = await _discord.Client.GetUserAsync(ban.LiftedById);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    embed.AddField("Was lifted by",
                        $"{(liftingMod is not null ? liftingMod.Mention : "Deleted or unavailable user")}");
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

            if (guildCfg.ModerationConfig.MemberEventsLogChannelId is null)
                return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Can't send messages in channel with Id: {guildCfg.ModerationConfig.MemberEventsLogChannelId}.");
            }

            return embed;
        }
    }
}