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

using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ReminderConfig;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using MikyM.Common.EfCore.ApplicationLayer.Interfaces;
using MikyM.Common.Utilities.Results;

namespace Lisbeth.Bot.Application.Services.Database.Interfaces;

public interface IGuildDataService : ICrudDataService<Guild, ILisbethBotDbContext>
{
    Task<Result<Guild>> AddConfigAsync(ModerationConfigReqDto req, bool shouldSave = false);
    Task<Result<Guild>> AddConfigAsync(TicketingConfigReqDto req, bool shouldSave = false);
    Task<Result<Guild>> AddConfigAsync(ReminderConfigReqDto req, bool shouldSave = false);
    Task<Result> DisableConfigAsync(ulong guildId, GuildModule type, bool shouldSave = false);
    Task<Result<Guild>> EnableConfigAsync(ulong guildId, GuildModule type, bool shouldSave = false);
    Task<Result> RepairModuleConfigAsync(TicketingConfigRepairReqDto req, bool shouldSave = false);
    Task<Result> RepairModuleConfigAsync(ModerationConfigRepairReqDto req, bool shouldSave = false);
    Task<Result> RepairModuleConfigAsync(ReminderConfigRepairReqDto req, bool shouldSave = false);
    Task<Result> EditTicketingConfigAsync(TicketingConfigEditReqDto req, bool shouldSave = false);
    Task<Result> EditModerationConfigAsync(ulong guildId, bool shouldSave = false);
    Task<Result> AddRoleMenuAsync(RoleMenuAddReqDto req, bool shouldSave = false);
}
