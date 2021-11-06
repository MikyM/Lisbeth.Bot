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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hangfire;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Results;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ban;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Discord.Interfaces;
using System;
using System.Globalization;
using System.Threading.Tasks;

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
        public async Task<Result> UnbanCheckAsync()
        {
            try
            {
                var res = await _banService.GetBySpecAsync<Ban>(
                    new ActiveExpiredBansInActiveGuildsSpecifications());

                if (!res.IsSuccess) return Result.FromSuccess();

                foreach (var ban in res.Entity)
                {
                    var req = new BanDisableReqDto(ban.UserId, ban.GuildId, _discord.Client.CurrentUser.Id);
                    await UnbanAsync(req);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Automatic unban failed with: {ex.GetFullMessage()}");
                return Result.FromError(new InvalidOperationError($"Automatic unban failed with: {ex.GetFullMessage()}"));
            }

            return Result.FromSuccess();
        }

        public async Task<Result<DiscordEmbed>> BanAsync(BanReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember target = await guild.GetMemberAsync(req.TargetUserId);
            DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await BanAsync(guild, target, moderator, req);
        }

        public async Task<Result<DiscordEmbed>> BanAsync(InteractionContext ctx, BanReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
                target = ctx.ResolvedUserMentions[0];
            else
                try
                {
                    target = await _discord.Client.GetUserAsync(req.TargetUserId);
                }
                catch (Exception)
                {
                    return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.User));
                }

            return await BanAsync(ctx.Guild, target, ctx.Member, req);
        }

        public async Task<Result<DiscordEmbed>> UnbanAsync(BanDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild;
            DiscordMember target;

            if (req.Id.HasValue)
            {
                var res = await _banService.GetAsync<Ban>(req.Id.Value);
                if (!res.IsSuccess) return Result<DiscordEmbed>.FromError(res);
                req.GuildId = res.Entity.GuildId;
                req.TargetUserId = res.Entity.UserId;
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

        public async Task<Result<DiscordEmbed>> UnbanAsync(InteractionContext ctx, BanDisableReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
                target = ctx.ResolvedUserMentions[0];
            else
                try
                {
                    if (req.TargetUserId is not null)
                    {
                        target = await _discord.Client.GetUserAsync(req.TargetUserId.Value);
                    }
                    else
                    {
                        var res = await _banService.GetAsync<Ban>(req.Id.Value);

                        if (res.IsSuccess)
                            target = await _discord.Client.GetUserAsync(res.Entity.UserId);
                        else
                            return Result<DiscordEmbed>.FromError(res);
                    }
                }
                catch (Exception)
                {
                    return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.User));
                }

            return await UnbanAsync(ctx.Guild, target, ctx.Member, req);
        }

        public async Task<Result<DiscordEmbed>> GetSpecificUserGuildBanAsync(BanGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordGuild guild;

            if (req.Id.HasValue)
            {
                var ban = await _banService.GetAsync<Ban>(req.Id.Value);
                if (!ban.IsSuccess) return Result<DiscordEmbed>.FromError(ban);
                req.GuildId = ban.Entity.GuildId;
                req.TargetUserId = ban.Entity.UserId;
                req.AppliedById = ban.Entity.AppliedById;
                req.LiftedById = ban.Entity.LiftedById;
                req.AppliedOn = ban.Entity.CreatedAt;
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

        public async Task<Result<DiscordEmbed>> GetSpecificUserGuildBanAsync(InteractionContext ctx, BanGetReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordUser target;

            if (ctx.ResolvedUserMentions is not null)
                target = ctx.ResolvedUserMentions[0];
            else
                try
                {
                    if (req.TargetUserId is not null)
                    {
                        target = await _discord.Client.GetUserAsync(req.TargetUserId.Value);
                    }
                    else
                    {
                        var res = await _banService.GetAsync<Ban>(req.Id.Value);

                        if (res.IsSuccess)
                            target = await _discord.Client.GetUserAsync(res.Entity.UserId);
                        else
                            return Result<DiscordEmbed>.FromError(res);
                    }
                }
                catch (Exception)
                {
                    return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.User));
                }

            return await GetAsync(ctx.Guild, target, ctx.Member, req);
        }

        private async Task<Result<DiscordEmbed>> BanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
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

            if (!moderator.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());
            if (targetMember is not null && targetMember.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

            DiscordBan ban;
            DiscordChannel channel = null;

            var guildCfg =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));

            if (!guildCfg.IsSuccess)
                return Result<DiscordEmbed>.FromError(guildCfg);

            if (guildCfg.Entity.ModerationConfig is null)
                return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.Entity.ModerationConfig)));

            try
            {
                channel = await _discord.Client.GetChannelAsync(guildCfg.Entity.ModerationConfig.MemberEventsLogChannelId);
            }
            catch (Exception)
            {
                return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
            }


            try
            {
                ban = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                ban = null;
            }

            var result = await _banService.AddOrExtendAsync(req, true);
            var (id, foundEntity) = result.Entity;

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

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Can't send messages in channel with Id: {guildCfg.Entity.ModerationConfig.MemberEventsLogChannelId}.");
            }

            return Result<DiscordEmbed>.FromSuccess(embed.Build());
        }

        private async Task<Result<DiscordEmbed>> UnbanAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
            BanDisableReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel channel = null;
            DiscordBan ban;

            var result =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));

            if (!result.IsSuccess)
                return Result<DiscordEmbed>.FromError(result);

            var guildCfg = result.Entity;

            if (guildCfg.ModerationConfig is null)
                return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.ModerationConfig)));

            try
            {
                channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId);
            }
            catch (Exception ex)
            {
                return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
            }

            if (!moderator.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

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

                if (res.IsSuccess)
                {
                    embed.WithFooter($"Case ID: {res.Entity.Id}  | Member ID: {target.Id}");
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
                embed.WithFooter($"Case ID: {res.Entity.Id} | Member ID: {target.Id}");
            }
            
            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                return Result<DiscordEmbed>.FromError(new DiscordInvalidOperationError());
            }

            return Result<DiscordEmbed>.FromSuccess(embed.Build());
        }

        private async Task<Result<DiscordEmbed>> GetAsync(DiscordGuild guild, DiscordUser target, DiscordMember moderator,
            BanGetReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));
            if (req is null) throw new ArgumentNullException(nameof(req));


            if (!moderator.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

            var result =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpecifications(guild.Id));

            if (!result.IsSuccess)
                return Result<DiscordEmbed>.FromError(result);

            var guildCfg = result.Entity;

            if (guildCfg.ModerationConfig is null)
                return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.ModerationConfig)));

            DiscordChannel channel = null;
            DiscordBan discordBan;

            try
            {
                channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId);
            }
            catch (Exception)
            {
                return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
            }


            try
            {
                discordBan = await guild.GetBanAsync(target.Id);
            }
            catch (Exception)
            {
                discordBan = null;
            }

            var res = await _banService.GetSingleBySpecAsync<Ban>(
                new BanBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn,
                    req.AppliedOn, req.LiftedById));
            
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            if (res.IsSuccess)
            {
                DiscordUser banningMod = null;
                try
                {
                    banningMod = await _discord.Client.GetUserAsync(res.Entity.AppliedById);
                }
                catch (Exception)
                {
                    // ignore
                }

                embed.WithAuthor($"Ban Info | {target.GetFullUsername()}", null, target.AvatarUrl);
                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", $"{(banningMod is not null ? banningMod.Mention : "Deleted user")}", true);
                embed.AddField("Banned until", res.Entity.AppliedUntil.ToString(), true);
                embed.AddField("Reason", res.Entity.Reason);
                if (res.Entity.LiftedById != 0)
                {
                    DiscordUser liftingMod = null;
                    try
                    {
                        if (req.LiftedById is not null) liftingMod = await _discord.Client.GetUserAsync(res.Entity.LiftedById);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    embed.AddField("Was lifted by",
                        $"{(liftingMod is not null ? liftingMod.Mention : "Deleted or unavailable user")}");
                }

                embed.WithFooter($"Case ID: {res.Entity.Id} | User ID: {target.Id}");
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

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                return Result<DiscordEmbed>.FromError(new DiscordInvalidOperationError());
            }

            return Result<DiscordEmbed>.FromSuccess(embed.Build());
        }
    }
}