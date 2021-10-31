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

using AutoMapper;
using Hangfire.Annotations;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class GuildService : CrudService<Guild, LisbethBotDbContext>, IGuildService
    {
        private readonly ICrudService<ModerationConfig, LisbethBotDbContext> _moderationService;
        private readonly ICrudService<TicketingConfig, LisbethBotDbContext> _ticketingService;

        public GuildService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof,
            ICrudService<ModerationConfig, LisbethBotDbContext> moderationService,
            ICrudService<TicketingConfig, LisbethBotDbContext> ticketingService) : base(mapper, uof)
        {
            _moderationService = moderationService;
            _ticketingService = ticketingService;
        }

        public async Task<Guild> AddConfigAsync(TicketingConfigReqDto req, bool shouldSave = false)
        {
            var guild = await base.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
            if (guild is null) throw new NotFoundException("Guild doesn't exist in the database");
            if (guild.TicketingConfig is not null && guild.TicketingConfig.IsDisabled)
                return await this.EnableConfigAsync(req.GuildId, GuildConfigType.Ticketing, shouldSave);
            else if (guild.TicketingConfig is not null && !guild.TicketingConfig.IsDisabled) throw new InvalidOperationException("Guild already has an enabled ticketing config");

            await _ticketingService.AddAsync(req, shouldSave);

            return guild;
        }

        public async Task<Guild> AddConfigAsync(ModerationConfigReqDto req, bool shouldSave = false)
        {
            var guild = await base.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpecifications(req.GuildId));
            if (guild is null) throw new NotFoundException("Guild doesn't exist in the database");
            if (guild.TicketingConfig is not null && guild.ModerationConfig.IsDisabled)
                return await this.EnableConfigAsync(req.GuildId, GuildConfigType.Moderation, shouldSave);
            if (guild.ModerationConfig is not null && !guild.ModerationConfig.IsDisabled) throw new InvalidOperationException("Guild already has an enabled moderation config");

            await _moderationService.AddAsync(req, shouldSave);

            return guild;
        }

        public async Task<bool> DisableConfigAsync(ulong guildId, GuildConfigType type, bool shouldSave = false)
        {
            Guild guild;
            switch (type)
            {
                case GuildConfigType.Ticketing:
                    guild = await base.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTicketingSpecifications(guildId));
                    if (guild?.TicketingConfig is null) throw new NotFoundException("Guild doesn't exist in the database or it does not have a ticketing config");
                    if (guild.TicketingConfig.IsDisabled) return false;

                    await _ticketingService.DisableAsync(guild.TicketingConfig, shouldSave);
                    break;
                case GuildConfigType.Moderation:
                    guild = await base.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithModerationSpecifications(guildId));
                    if (guild?.ModerationConfig is null) throw new NotFoundException("Guild doesn't exist in the database or it does not have a moderation config");
                    if (guild.ModerationConfig.IsDisabled) return false;

                    await _moderationService.DisableAsync(guild.ModerationConfig, shouldSave);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return true;
        }

        public async Task<Guild> EnableConfigAsync(ulong guildId, GuildConfigType type, bool shouldSave = false)
        {
            Guild guild;
            switch (type)
            {
                case GuildConfigType.Ticketing:
                    guild = await base.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTicketingSpecifications(guildId));
                    if (guild?.TicketingConfig is null) throw new NotFoundException("Guild doesn't exist in the database or it does not have a ticketing config");
                    if (!guild.TicketingConfig.IsDisabled) return null;

                    _ticketingService.BeginUpdate(guild.TicketingConfig);
                    guild.TicketingConfig.IsDisabled = false;

                    if (shouldSave) await _ticketingService.CommitAsync();
                    break;
                case GuildConfigType.Moderation:
                    guild = await base.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithModerationSpecifications(guildId));
                    if (guild?.ModerationConfig is null) throw new NotFoundException("Guild doesn't exist in the database or it does not have a moderation config");
                    if (!guild.ModerationConfig.IsDisabled) return null;

                    base.BeginUpdate(guild.ModerationConfig);
                    guild.ModerationConfig.IsDisabled = false;

                    if (shouldSave) await _moderationService.CommitAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return guild;
        }

        public async Task EditTicketingConfigAsync(TicketingConfigEditReqDto req, bool shouldSave = false)
        {
            var guild = await base.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(req.GuildId));
            if (guild?.TicketingConfig is null) throw new NotFoundException("Guild doesn't exist in the database or it does not have a ticketing config");
            if (guild.TicketingConfig.IsDisabled) throw new DisabledEntityException("Guild's ticketing config is disabled, please re-enable it first.");

            _ticketingService.BeginUpdate(guild.TicketingConfig);
            guild.TicketingConfig.CleanAfter = req.CleanAfter;
            guild.TicketingConfig.CloseAfter = req.CloseAfter;
            guild.TicketingConfig.ClosedNamePrefix = req.ClosedNamePrefix;
            guild.TicketingConfig.OpenedNamePrefix = req.OpenedNamePrefix;

            if (shouldSave) await _ticketingService.CommitAsync();
        }

        public Task EditModerationConfigAsync(ulong guildId, bool shouldSave = false)
        {
            throw new NotImplementedException();
        }
    }
}