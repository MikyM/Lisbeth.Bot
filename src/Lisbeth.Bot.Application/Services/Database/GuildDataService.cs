// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using AutoMapper;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ReminderConfig;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using MikyM.Common.EfCore.ApplicationLayer.Interfaces;
using MikyM.Common.EfCore.DataAccessLayer.UnitOfWork;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class GuildDataService : CrudDataService<Guild, ILisbethBotDbContext>, IGuildDataService
{
    private readonly ICrudDataService<ModerationConfig, ILisbethBotDbContext> _moderationService;
    private readonly ICrudDataService<RoleMenu, ILisbethBotDbContext> _roleMenuService;
    private readonly ICrudDataService<TicketingConfig, ILisbethBotDbContext> _ticketingService;

    public GuildDataService(IMapper mapper, IUnitOfWork<ILisbethBotDbContext> uof,
        ICrudDataService<ModerationConfig, ILisbethBotDbContext> moderationService,
        ICrudDataService<TicketingConfig, ILisbethBotDbContext> ticketingService,
        ICrudDataService<RoleMenu, ILisbethBotDbContext> roleMenuService) : base(mapper, uof)
    {
        _moderationService = moderationService;
        _ticketingService = ticketingService;
        _roleMenuService = roleMenuService;
    }

    public async Task<Result<Guild>> AddConfigAsync(TicketingConfigReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
        if (!result.IsDefined()) return Result<Guild>.FromError(new NotFoundError());
        if (result.Entity.TicketingConfig is not null && result.Entity.TicketingConfig.IsDisabled)
            return await EnableConfigAsync(req.GuildId, GuildModule.Ticketing, shouldSave);
        if (result.Entity.TicketingConfig is not null && !result.Entity.TicketingConfig.IsDisabled)
            return Result<Guild>.FromError(new InvalidOperationError());

        await _ticketingService.AddAsync(req, shouldSave);

        return result.Entity;
    }

    public async Task<Result<Guild>> AddConfigAsync(ReminderConfigReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync(
            new ActiveGuildByIdSpec(req.GuildId));
        if (!result.IsDefined(out var guild)) return Result<Guild>.FromError(result);
        if (guild.IsReminderModuleEnabled)
            return new InvalidOperationError("Reminder module is already enabled");

        BeginUpdate(guild);
        guild.ReminderChannelId = req.ChannelId;

        if (shouldSave) await CommitAsync();

        return await EnableConfigAsync(req.GuildId, GuildModule.Reminders, shouldSave);
    }

    public async Task<Result<Guild>> AddConfigAsync(ModerationConfigReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(req.GuildId));
        if (!result.IsDefined()) throw new NotFoundException("Guild doesn't exist in the database");
        if (result.Entity.ModerationConfig is not null && result.Entity.ModerationConfig.IsDisabled)
            return await EnableConfigAsync(req.GuildId, GuildModule.Moderation, shouldSave);
        if (result.Entity.ModerationConfig is not null && !result.Entity.ModerationConfig.IsDisabled)
            return Result<Guild>.FromError(new InvalidOperationError());

        await _moderationService.AddAsync(req, shouldSave);

        return result.Entity;
    }

    public async Task<Result> DisableConfigAsync(ulong guildId, GuildModule type, bool shouldSave = false)
    {
        Result<Guild> result;
        switch (type)
        {
            case GuildModule.Ticketing:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guildId));
                if (!result.IsDefined() || result.Entity.TicketingConfig is null)
                    return Result.FromError(new NotFoundError());
                if (result.Entity.IsDisabled)
                    return Result.FromError(new DisabledEntityError(nameof(result.Entity.TicketingConfig)));

                await _ticketingService.DisableAsync(result.Entity.TicketingConfig, shouldSave);
                break;
            case GuildModule.Moderation:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithModerationSpec(guildId));
                if (!result.IsDefined() || result.Entity.ModerationConfig is null)
                    return Result.FromError(new NotFoundError());
                if (result.Entity.IsDisabled)
                    return Result.FromError(new DisabledEntityError(nameof(result.Entity.ModerationConfig)));

                await _moderationService.DisableAsync(result.Entity.ModerationConfig, shouldSave);
                break;
            case GuildModule.Reminders:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByIdSpec(guildId));
                if (!result.IsDefined(out var guild))
                    return Result.FromError(result);
                if (!guild.IsReminderModuleEnabled)
                    return new DisabledEntityError(nameof(guild.ReminderChannelId));

                BeginUpdate(guild);
                guild.ReminderChannelId = null;
                if (shouldSave) await base.CommitAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return Result.FromSuccess();
    }

    public async Task<Result<Guild>> EnableConfigAsync(ulong guildId, GuildModule type, bool shouldSave = false)
    {
        Result<Guild> result;
        switch (type)
        {
            case GuildModule.Ticketing:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guildId));
                if (!result.IsDefined() || result.Entity.TicketingConfig is null)
                    return Result<Guild>.FromError(new NotFoundError());
                if (!result.Entity.IsDisabled)
                    return Result<Guild>.FromError(new DisabledEntityError(nameof(result.Entity.TicketingConfig)));

                _ticketingService.BeginUpdate(result.Entity.TicketingConfig);
                result.Entity.TicketingConfig.IsDisabled = false;

                if (shouldSave) await _ticketingService.CommitAsync();
                break;
            case GuildModule.Moderation:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithModerationSpec(guildId));
                if (!result.IsDefined() || result.Entity.ModerationConfig is null)
                    return Result<Guild>.FromError(new NotFoundError());
                if (!result.Entity.IsDisabled)
                    return Result<Guild>.FromError(new DisabledEntityError(nameof(result.Entity.ModerationConfig)));

                base.BeginUpdate(result.Entity.ModerationConfig);
                result.Entity.ModerationConfig.IsDisabled = false;

                if (shouldSave) await _moderationService.CommitAsync();
                break;
            case GuildModule.Reminders:
                result = await base.GetSingleBySpecAsync(
                    new ActiveGuildByIdSpec(guildId));
                // ignored for now
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return result.Entity;
    }

    public async Task<Result> EditTicketingConfigAsync(TicketingConfigEditReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
        if (!result.IsDefined() || result.Entity.TicketingConfig is null)
            return Result.FromError(new NotFoundError());
        if (result.Entity.TicketingConfig.IsDisabled)
            return Result.FromError(new DisabledEntityError(nameof(result.Entity.TicketingConfig)));

        _ticketingService.BeginUpdate(result.Entity.TicketingConfig);
        result.Entity.TicketingConfig.CleanAfter = req.CleanAfter;
        result.Entity.TicketingConfig.CloseAfter = req.CloseAfter;
        result.Entity.TicketingConfig.ClosedNamePrefix = req.ClosedNamePrefix;
        result.Entity.TicketingConfig.OpenedNamePrefix = req.OpenedNamePrefix;

        if (shouldSave) await _ticketingService.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RepairModuleConfigAsync(TicketingConfigRepairReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
        if (!result.IsDefined() || result.Entity.TicketingConfig is null)
            return Result.FromError(new NotFoundError());
        if (result.Entity.TicketingConfig.IsDisabled)
            return Result.FromError(new DisabledEntityError(nameof(result.Entity.TicketingConfig)));

        _ticketingService.BeginUpdate(result.Entity.TicketingConfig);
        if (req.ClosedCategoryId is not null)
            result.Entity.TicketingConfig.ClosedCategoryId = req.ClosedCategoryId.Value;
        if (req.OpenedCategoryId is not null)
            result.Entity.TicketingConfig.OpenedCategoryId = req.OpenedCategoryId.Value;
        if (req.LogChannelId is not null) result.Entity.TicketingConfig.LogChannelId = req.LogChannelId.Value;

        if (shouldSave) await _ticketingService.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RepairModuleConfigAsync(ReminderConfigRepairReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
        if (!result.IsDefined() || result.Entity.TicketingConfig is null)
            return Result.FromError(new NotFoundError());
        if (result.Entity.TicketingConfig.IsDisabled)
            return Result.FromError(new DisabledEntityError(nameof(result.Entity.ReminderChannelId)));

        base.BeginUpdate(result.Entity);
        result.Entity.ReminderChannelId = req.ChannelId;

        if (shouldSave) await _ticketingService.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RepairModuleConfigAsync(ModerationConfigRepairReqDto req, bool shouldSave = false)
    {
        var result = await base.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
        if (!result.IsDefined() || result.Entity.ModerationConfig is null)
            return Result.FromError(new NotFoundError());
        if (result.Entity.ModerationConfig.IsDisabled)
            return Result.FromError(new DisabledEntityError(nameof(result.Entity.ModerationConfig)));

        _moderationService.BeginUpdate(result.Entity.ModerationConfig);
        if (req.MemberEventsLogChannelId is not null)
            result.Entity.ModerationConfig.MemberEventsLogChannelId = req.MemberEventsLogChannelId.Value;
        if (req.MessageDeletedEventsLogChannelId is not null)
            result.Entity.ModerationConfig.MessageDeletedEventsLogChannelId =
                req.MessageDeletedEventsLogChannelId.Value;
        if (req.MessageUpdatedEventsLogChannelId is not null)
            result.Entity.ModerationConfig.MessageUpdatedEventsLogChannelId =
                req.MessageUpdatedEventsLogChannelId.Value;
        if (req.ModerationLogChannelId is not null)
            result.Entity.ModerationConfig.ModerationLogChannelId = req.ModerationLogChannelId.Value;
        if (req.MuteRoleId is not null) result.Entity.ModerationConfig.MuteRoleId = req.MuteRoleId.Value;

        if (shouldSave) await _moderationService.CommitAsync();

        return Result.FromSuccess();
    }

    public Task<Result> EditModerationConfigAsync(ulong guildId, bool shouldSave = false)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> AddRoleMenuAsync(RoleMenuAddReqDto req, bool shouldSave = false)
    {
        var result = await GetSingleBySpecAsync<Guild>(
            new ActiveGuildByIdSpec(req.GuildId));
        if (!result.IsSuccess) return Result.FromError(new NotFoundError());

        var partial = await _roleMenuService.AddAsync(req, shouldSave);

        return partial.IsSuccess ? Result.FromSuccess() : Result.FromError(partial.Error);
    }
}
