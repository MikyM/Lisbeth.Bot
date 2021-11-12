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

using System.Globalization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Microsoft.Extensions.Logging;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordMuteService : IDiscordMuteService
{
    private readonly IDiscordService _discord;
    private readonly IGuildService _guildService;
    private readonly ILogger<DiscordMuteService> _logger;
    private readonly IMuteService _muteService;

    public DiscordMuteService(IDiscordService discord, IGuildService guildService, IMuteService muteService,
        ILogger<DiscordMuteService> logger)
    {
        _discord = discord;
        _guildService = guildService;
        _muteService = muteService;
        _logger = logger;
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(MuteReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
        DiscordMember target = await guild.GetMemberAsync(req.TargetUserId);
        DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await MuteAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(InteractionContext ctx, MuteReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await MuteAsync(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id),
            ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(ContextMenuContext ctx, MuteReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await MuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(MuteDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild;
        DiscordMember target;

        if (req.Id.HasValue)
        {
            var result = await _muteService.GetAsync<Mute>(req.Id.Value);
            if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(new NotFoundError());
            req.GuildId = result.Entity.GuildId;
            req.TargetUserId = result.Entity.UserId;
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

        return await UnmuteAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(InteractionContext ctx, MuteDisableReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await UnmuteAsync(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id),
            ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(ContextMenuContext ctx, MuteDisableReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await UnmuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(MuteGetReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordMember target;
        DiscordGuild guild;

        if (req.Id.HasValue)
        {
            var result = await _muteService.GetAsync<Mute>(req.Id.Value);
            if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(new NotFoundError());
            var mute = result.Entity;
            req.GuildId = mute.GuildId;
            req.TargetUserId = mute.UserId;
            req.AppliedById = mute.AppliedById;
            req.LiftedById = mute.LiftedById;
            req.AppliedOn = mute.CreatedAt;
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

        return await GetSpecificUserGuildMuteAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(InteractionContext ctx, MuteGetReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await GetSpecificUserGuildMuteAsync(ctx.Guild,
            await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id),
            ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(ContextMenuContext ctx, MuteGetReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await GetSpecificUserGuildMuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member, req);
    }

    [Queue("moderation")]
    [PreserveOriginalQueue]
    public async Task<Result> UnmuteCheckAsync()
    {
        try
        {
            var res = await _muteService.GetBySpecAsync<Mute>(
                new ActiveExpiredMutesInActiveGuildsSpecifications());

            if (!res.IsDefined() || res.Entity.Count == 0) return Result.FromSuccess();

            foreach (var mute in res.Entity)
            {
                var req = new MuteDisableReqDto(mute.UserId, mute.GuildId, _discord.Client.CurrentUser.Id);
                await UnmuteAsync(req);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Automatic unmute failed with: {ex.GetFullMessage()}");
            return Result.FromError(
                new InvalidOperationError($"Automatic unmute failed with: {ex.GetFullMessage()}"));
        }

        return Result.FromSuccess();
    }

    private async Task<Result<DiscordEmbed>> MuteAsync(DiscordGuild guild, DiscordMember target,
        DiscordMember moderator,
        MuteReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordChannel? channel;

        var result =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new Specification<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));

        if (!result.IsDefined())
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Guild));

        var guildCfg = result.Entity;

        if (guildCfg.ModerationConfig is null)
            return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.ModerationConfig)));

        try
        {
            channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId);
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
        }

        if (req.AppliedUntil < DateTime.UtcNow)
            return Result<DiscordEmbed>.FromError(new ArgumentOutOfRangeError(nameof(req.AppliedUntil)));

        if (!moderator.IsModerator() || target.IsModerator())
            return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

        TimeSpan tmspDuration = req.AppliedUntil.Subtract(DateTime.UtcNow);

        string lengthString = req.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

        var partial = await _muteService.AddOrExtendAsync(req, true);
        var (id, foundEntity) = partial.Entity;

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(0x18315C);
        embed.WithAuthor($"Mute | {target.GetFullDisplayName()}", null, target.AvatarUrl);

        bool isMuted = target.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;
        bool resMute = true;

        if (foundEntity is null)
        {
            if (!isMuted)
                resMute = await target.Mute(guildCfg.ModerationConfig.MuteRoleId);

            if (!resMute)
            {
                var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                embed.WithColor(0x18315C);
                embed.WithAuthor($"{noEntryEmoji} Mute denied");
                embed.WithDescription("Can't mute other moderators.");
            }
            else
            {
                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("Length", lengthString, true);
                embed.AddField("Muted until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                embed.AddField("Reason", req.Reason);
                embed.WithFooter($"Case ID: {id} | Member ID: {target.Id}");
            }
        }
        else
        {
            DiscordUser? previousMod;
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
                    resMute = await target.Mute(guildCfg.ModerationConfig.MuteRoleId);

                if (!resMute)
                {
                    var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                    embed.WithColor(0x18315C);
                    embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                    embed.WithDescription("Can't mute other moderators.");
                }
                else
                {
                    embed.WithDescription(
                        $"This user has already been muted until {foundEntity.AppliedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                    embed.WithFooter($"Case ID: {id} | Member ID: {foundEntity.UserId}");
                }
            }
            else
            {
                if (!isMuted)
                    resMute = await target.Mute(guildCfg.ModerationConfig.MuteRoleId);

                if (!resMute)
                {
                    var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                    embed.WithColor(0x18315C);
                    embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                    embed.WithDescription("Can't mute other moderators.");
                }
                else
                {
                    embed.WithAuthor($"Extend Mute | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.AddField("Previous mute until", foundEntity.AppliedUntil.ToString(), true);
                    embed.AddField("Previous moderator",
                        $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                    embed.AddField("Previous reason", foundEntity.Reason, true);
                    embed.AddField("User mention", target.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Muted until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {id} | Member ID: {target.Id}");
                }
            }
        }

        // means we're logging to log channel and returning an embed for interaction or other purposes

        try
        {
            if (channel is not null) await channel.SendMessageAsync(embed.Build());
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordInvalidOperationError());
        }

        return Result<DiscordEmbed>.FromSuccess(embed.Build());
    }

    private async Task<Result<DiscordEmbed>> UnmuteAsync(DiscordGuild guild, DiscordMember target,
        DiscordMember moderator,
        MuteDisableReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (moderator.Guild.Id != guild.Id) throw new ArgumentException(nameof(moderator));
        if (target.Guild.Id != guild.Id) throw new ArgumentException(nameof(target));

        if (!moderator.IsModerator())
            return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

        DiscordChannel? channel;

        var result =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new Specification<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));

        if (!result.IsDefined())
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Guild));

        var guildCfg = result.Entity;

        if (guildCfg.ModerationConfig is null)
            return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.ModerationConfig)));

        try
        {
            channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId);
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(0x18315C);

        bool isMuted = target.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;

        var res = await _muteService.DisableAsync(req);

        if (!res.IsDefined())
        {
            if (isMuted)
            {
                await target.Unmute(guildCfg.ModerationConfig.MuteRoleId);
                embed.WithAuthor($"Unmute | {target.GetFullUsername()}", null, target.AvatarUrl);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("User mention", target.Mention, true);
                embed.WithDescription("Successfully unmuted");
                embed.WithFooter($"Case ID: unknown | Member ID: {target.Id}");
            }
            else
            {
                embed.WithAuthor($"Unmute failed | {target.GetFullDisplayName()}", null, target.AvatarUrl);
                embed.WithDescription("This user isn't currently muted.");
                embed.WithFooter($"Case ID: unknown | Member ID: {target.Id}");
            }
        }
        else
        {
            await _muteService.CommitAsync();

            if (isMuted)
                await target.Unmute(guildCfg.ModerationConfig.MuteRoleId);

            embed.WithAuthor($"Unmute | {target.GetFullDisplayName()}", null, target.AvatarUrl);
            embed.AddField("Moderator", moderator.Mention, true);
            embed.AddField("User mention", target.Mention, true);
            embed.WithDescription("Successfully unmuted");
            embed.WithFooter($"Case ID: {res.Entity.Id} | Member ID: {target.Id}");
        }

        // means we're logging to log channel and returning an embed for interaction or other purposes

        try
        {
            if (channel is not null) await channel.SendMessageAsync(embed.Build());
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordInvalidOperationError());
        }

        return Result<DiscordEmbed>.FromSuccess(embed.Build());
    }

    private async Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(DiscordGuild guild, DiscordMember target,
        DiscordMember moderator, MuteGetReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));


        if (!moderator.IsModerator())
            return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

        DiscordChannel? channel;

        var result =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new Specification<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));

        if (!result.IsDefined())
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Guild));

        var guildCfg = result.Entity;

        if (guildCfg.ModerationConfig is null)
            return Result<DiscordEmbed>.FromError(new DisabledEntityError(nameof(guildCfg.ModerationConfig)));

        try
        {
            channel = await _discord.Client.GetChannelAsync(guildCfg.ModerationConfig.MemberEventsLogChannelId);
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
        }


        var res = await _muteService.GetSingleBySpecAsync<Mute>(
            new MuteBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn,
                req.AppliedOn, req.LiftedById));

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(0x18315C);

        if (res.IsDefined())
        {
            var mute = res.Entity;
            DiscordUser? mutingMod = null;
            try
            {
                mutingMod = await _discord.Client.GetUserAsync(mute.AppliedById);
            }
            catch (Exception)
            {
                // ignore
            }

            embed.WithAuthor($"Mute Info | {target.GetFullDisplayName()}", null, target.AvatarUrl);
            embed.AddField("User mention", target.Mention, true);
            embed.AddField("Moderator", $"{(mutingMod is not null ? mutingMod.Mention : "Deleted user")}", true);
            embed.AddField("Muted until", mute.AppliedUntil.ToString(), true);
            embed.AddField("Reason", mute.Reason);
            if (mute.LiftedById != 0)
            {
                DiscordUser? liftingMod = null;
                try
                {
                    liftingMod = await _discord.Client.GetUserAsync(mute.LiftedById);
                }
                catch (Exception)
                {
                    // ignore
                }

                embed.AddField("Was lifted by",
                    $"{(liftingMod is not null ? liftingMod.Mention : "Deleted user")}");
            }

            embed.WithFooter($"Case ID: {mute.Id} | User ID: {target.Id}");
        }
        else
        {
            embed.WithDescription("No mute info found.");
            embed.WithFooter($"Case ID: unknown | User ID: {target.Id}");
        }

        // means we're logging to log channel and returning an embed for interaction or other purposes

        try
        {
            if (channel is not null) await channel.SendMessageAsync(embed.Build());
        }
        catch (Exception)
        {
            return Result<DiscordEmbed>.FromError(new DiscordInvalidOperationError());
        }

        return Result<DiscordEmbed>.FromSuccess(embed.Build());
    }
}