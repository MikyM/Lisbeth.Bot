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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hangfire;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordMuteService : IDiscordMuteService
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordMuteService> _logger;
    private readonly IMuteService _muteService;
    private readonly IDiscordGuildLoggerService _guildLogger;
    private readonly IResponseDiscordEmbedBuilder _embedBuilder;

    public DiscordMuteService(IDiscordService discord, IGuildDataService guildDataService, ILogger<DiscordMuteService> logger,
        IMuteService muteService, IDiscordGuildLoggerService guildLogger, IDiscordEmbedProvider embedProvider, IResponseDiscordEmbedBuilder embedBuilder)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _muteService = muteService;
        _guildLogger = guildLogger;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(MuteApplyReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
        DiscordMember target = await guild.GetMemberAsync(req.TargetUserId);
        DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await MuteAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(InteractionContext ctx, MuteApplyReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await MuteAsync(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id),
            ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> MuteAsync(ContextMenuContext ctx, MuteApplyReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await MuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(MuteRevokeReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild;
        DiscordMember target;

        if (req.Id.HasValue)
        {
            var result = await _muteService.GetAsync(req.Id.Value);
            if (!result.IsDefined()) return new NotFoundError();
            req.GuildId = result.Entity.GuildId;
            req.TargetUserId = result.Entity.UserId;
        }

        if (req.TargetUserId.HasValue)
        {
            guild = await _discord.Client.GetGuildAsync(req.GuildId);
            target = await guild.GetMemberAsync(req.TargetUserId.Value);
        }
        else
        {
            throw new InvalidOperationException();
        }

        DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await UnmuteAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(InteractionContext ctx, MuteRevokeReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await UnmuteAsync(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id),
            ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> UnmuteAsync(ContextMenuContext ctx, MuteRevokeReqDto req)
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
            var result = await _muteService.GetAsync(req.Id.Value);
            if (!result.IsDefined()) return new NotFoundError();
            var mute = result.Entity;
            req.GuildId = mute.GuildId;
            req.TargetUserId = mute.UserId;
            req.AppliedById = mute.AppliedById;
            req.LiftedById = mute.LiftedById;
            req.AppliedOn = mute.CreatedAt;
        }

        if (req.TargetUserId.HasValue)
        {
            guild = await _discord.Client.GetGuildAsync(req.GuildId);
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
            (DiscordMember)ctx.ResolvedUserMentions[0],
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
            var res = await _muteService.GetBySpecAsync(
                new ActiveExpiredMutesInActiveGuildsSpecifications());

            if (!res.IsDefined() || res.Entity.Count == 0) return Result.FromSuccess();

            foreach (var mute in res.Entity)
            {
                var req = new MuteRevokeReqDto(mute.UserId, mute.GuildId, _discord.Client.CurrentUser.Id);
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
        MuteApplyReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        var result =
            await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        if (!guild.RoleExists(guildEntity.ModerationConfig.MuteRoleId, out var mutedRole)) return new DiscordNotFoundError("Mute role not found.");

        if (req.AppliedUntil < DateTime.UtcNow)
            return new ArgumentOutOfRangeError(nameof(req.AppliedUntil));

        if (!moderator.IsModerator() || target.IsModerator())
            return new DiscordNotAuthorizedError();

        if (!guild.IsRoleHierarchyValid(mutedRole)) return new DiscordError("Bots role is below muted role in the role hierarchy.");
        if (!guild.HasSelfPermissions(Permissions.ManageRoles)) return new DiscordError("Bot doesn't have manage roles permission.");


        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.Mute, moderator, target, guildEntity.EmbedHexColor);

        var resMute = await target.MuteAsync(guildEntity.ModerationConfig.MuteRoleId);
        if (!resMute.IsSuccess) return new DiscordError("Failed to mute.");

        var partial = await _muteService.AddOrExtendAsync(req, true);
        if (!partial.IsDefined(out var idEntityPair)) return Result<DiscordEmbed>.FromError(partial);
        
        return _embedBuilder
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModAddReqResponseEnricher(req, target, idEntityPair.FoundEntity))
            .WithCase(idEntityPair.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }

    private async Task<Result<DiscordEmbed>> UnmuteAsync(DiscordGuild guild, DiscordMember target,
        DiscordMember moderator,
        MuteRevokeReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (moderator.Guild.Id != guild.Id) throw new ArgumentException(nameof(moderator));
        if (target.Guild.Id != guild.Id) throw new ArgumentException(nameof(target));

        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        var result =
            await _guildDataService.GetSingleBySpecAsync<Guild>(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        if (!guild.RoleExists(guildEntity.ModerationConfig.MuteRoleId, out var mutedRole)) return new DiscordNotFoundError("Mute role not found.");
        if (!guild.IsRoleHierarchyValid(mutedRole)) return new DiscordError("Bots role is below muted role in the role hierarchy.");
        if (!guild.HasSelfPermissions(Permissions.ManageRoles)) return new DiscordError("Bot doesn't have manage roles permission.");

        bool isMuted = target.Roles.FirstOrDefault(r => r.Id == guildEntity.ModerationConfig.MuteRoleId) is not null;

        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.Unmute, moderator, target, guildEntity.EmbedHexColor);

        if (isMuted)
        {
            var muteRes = await target.UnmuteAsync(guildEntity.ModerationConfig.MuteRoleId);
            if (!muteRes.IsSuccess) return new DiscordError("Failed to unmute");
        }

        var res = await _muteService.DisableAsync(req, true);

        if (!res.IsDefined(out var foundMute)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModDisableReqResponseEnricher(req, target))
            .WithCase(foundMute.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }

    private async Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(DiscordGuild guild, DiscordMember target,
        DiscordMember moderator, MuteGetReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));


        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        var result =
            await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        var res = await _muteService.GetSingleBySpecAsync(
            new MuteBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn,
                req.AppliedOn, req.LiftedById));

        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.MuteGet, moderator, target, guildEntity.EmbedHexColor);

        if (!res.IsDefined(out var foundMute)) return new NotFoundError();

        return _embedBuilder
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModGetReqResponseEnricher(foundMute))
            .WithCase(foundMute.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }
}