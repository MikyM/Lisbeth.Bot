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
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ban;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[AutofacLifetimeScope(LifetimeScope.InstancePerLifetimeScope)]
public class DiscordBanService : IDiscordBanService
{
    private readonly IBanDataService _banDataService;
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordBanService> _logger;
    private readonly IResponseDiscordEmbedBuilder<DiscordModeration> _embedBuilder;
    private readonly IDiscordGuildLoggerService _guildLogger;

    public DiscordBanService(IBanDataService banDataService, IDiscordService discord, IGuildDataService guildDataService,
        ILogger<DiscordBanService> logger, IResponseDiscordEmbedBuilder<DiscordModeration> embedBuilder, IDiscordGuildLoggerService guildLogger)
    {
        _banDataService = banDataService;
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _embedBuilder = embedBuilder;
        _guildLogger = guildLogger;
        _guildDataService = guildDataService;
    }

    [Queue("moderation")]
    [PreserveOriginalQueue]
    public async Task<Result> UnbanCheckAsync()
    {
        try
        {
            var res = await _banDataService.GetBySpecAsync<Ban>(
                new ActiveExpiredBansInActiveGuildsSpecifications());

            if (!res.IsDefined()) return Result.FromSuccess();

            foreach (var ban in res.Entity)
            {
                var req = new BanRevokeReqDto(ban.UserId, ban.GuildId, _discord.Client.CurrentUser.Id);
                await UnbanAsync(req);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Automatic unban failed with: {ex.GetFullMessage()}");
            return new InvalidOperationError($"Automatic unban failed with: {ex.GetFullMessage()}");
        }

        return Result.FromSuccess();
    }

    public async Task<Result<DiscordEmbed>> BanAsync(BanApplyReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
        DiscordMember target = await guild.GetMemberAsync(req.TargetUserId);
        DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await BanAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> BanAsync(InteractionContext ctx, BanApplyReqDto req)
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
                return new DiscordNotFoundError(DiscordEntity.User);
            }

        return await BanAsync(ctx.Guild, target, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> UnbanAsync(BanRevokeReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild? guild = null;
        DiscordMember? target = null;

        if (req.Id.HasValue)
        {
            var res = await _banDataService.GetAsync(req.Id.Value);
            if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);
            req.GuildId = res.Entity.GuildId;
            req.TargetUserId = res.Entity.UserId;
        }
        else
        {
            guild = await _discord.Client.GetGuildAsync(req.GuildId);
            target = await guild.GetMemberAsync(req.TargetUserId);
        }

        if (guild is null || target is null) throw new DiscordNotFoundException();

        DiscordMember moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await UnbanAsync(guild, target, moderator, req);
    }

    public async Task<Result<DiscordEmbed>> UnbanAsync(InteractionContext ctx, BanRevokeReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordUser target;

        if (ctx.ResolvedUserMentions is not null)
            target = ctx.ResolvedUserMentions[0];
        else
            try
            {
                if (req.Id is not null)
                {
                    var res = await _banDataService.GetAsync(req.Id ?? throw new InvalidOperationException());

                    if (res.IsDefined())
                        target = await _discord.Client.GetUserAsync(res.Entity.UserId);
                    else
                        return Result<DiscordEmbed>.FromError(res);
                }
                else
                {
                    target = await _discord.Client.GetUserAsync(req.TargetUserId);
                }
            }
            catch (Exception)
            {
                return new DiscordNotFoundError(DiscordEntity.User);
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
            var ban = await _banDataService.GetAsync(req.Id.Value);
            if (!ban.IsDefined()) return Result<DiscordEmbed>.FromError(ban);
            req.GuildId = ban.Entity.GuildId;
            req.TargetUserId = ban.Entity.UserId;
            req.AppliedById = ban.Entity.AppliedById;
            req.LiftedById = ban.Entity.LiftedById;
            req.AppliedOn = ban.Entity.CreatedAt;
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

        return await GetSpecificUserBanAsync(guild, target, moderator, req);
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
                    var res = await _banDataService.GetAsync(req.Id ?? throw new InvalidOperationException());

                    if (res.IsDefined())
                        target = await _discord.Client.GetUserAsync(res.Entity.UserId);
                    else
                        return Result<DiscordEmbed>.FromError(res);
                }
            }
            catch (Exception)
            {
                return new DiscordNotFoundError(DiscordEntity.User);
            }

        return await GetSpecificUserBanAsync(ctx.Guild, target, ctx.Member, req);
    }

    private async Task<Result<DiscordEmbed>> BanAsync(DiscordGuild guild, DiscordUser target,
        DiscordMember moderator,
        BanApplyReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (target is null) throw new ArgumentNullException(nameof(target));

        if (moderator.Guild.Id != guild.Id) throw new ArgumentException(nameof(moderator));

        DiscordMember? targetMember;
        try
        {
            targetMember = await guild.GetMemberAsync(target.Id);
        }
        catch (Exception)
        {
            targetMember = null;
        }

        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();
        if (targetMember is not null && targetMember.IsModerator())
            return new DiscordNotAuthorizedError();

        DiscordBan? ban;

        var guildCfg =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!guildCfg.IsDefined(out var guildEntity))
            return Result<DiscordEmbed>.FromError(guildCfg);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.Ban, moderator, target, guildEntity.EmbedHexColor);

        try
        {
            ban = await guild.GetBanAsync(target.Id);
        }
        catch (Exception)
        {
            ban = null;
        }

        if (ban is null) await guild.BanMemberAsync(target.Id, 1, req.Reason);

        var partial = await _banDataService.AddOrExtendAsync(req, true);

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

    private async Task<Result<DiscordEmbed>> UnbanAsync(DiscordGuild guild, DiscordUser target,
        DiscordMember moderator,
        BanRevokeReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordBan? ban;

        var result =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return Result<DiscordEmbed>.FromError(result);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.Unban, moderator, target, guildEntity.EmbedHexColor);

        try
        {
            ban = await guild.GetBanAsync(target.Id);
        }
        catch (Exception)
        {
            ban = null;
        }

        if (ban is not null)
            try
            {
                await guild.UnbanMemberAsync(target.Id);
            }
            catch (Exception)
            {
                return new DiscordError("Failed to unban");
            }

        var res = await _banDataService.DisableAsync(req, true);

        if (!res.IsDefined(out var foundBan)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModDisableReqResponseEnricher(req, target))
            .WithCase(foundBan.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }

    private async Task<Result<DiscordEmbed>> GetSpecificUserBanAsync(DiscordGuild guild, DiscordUser target,
        DiscordMember moderator,
        BanGetReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));


        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        var result =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return Result<DiscordEmbed>.FromError(result);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        await _guildLogger.LogToDiscordAsync(guild, req, DiscordModeration.BanGet, moderator, target, guildEntity.EmbedHexColor);

        var res = await _banDataService.GetSingleBySpecAsync<Ban>(
            new BanBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn,
                req.AppliedOn, req.LiftedById));


        if (!res.IsDefined(out var foundBan)) return new NotFoundError();

        return _embedBuilder
            .WithCase(foundBan.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .AsEnriched<ResponseDiscordEmbedBuilder<DiscordModeration>>()
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModGetReqResponseEnricher(foundBan))
            .Build();
    }
}