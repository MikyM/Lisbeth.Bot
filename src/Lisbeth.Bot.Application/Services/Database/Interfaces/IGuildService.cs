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

using System.Threading.Tasks;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Services.Database.Interfaces
{
    public interface IGuildService : ICrudService<Guild, LisbethBotDbContext>
    {
        Task<Guild> AddConfigAsync(ModerationConfigReqDto req, bool shouldSave = false);
        Task<Guild> AddConfigAsync(TicketingConfigReqDto req, bool shouldSave = false);
        Task<bool> DisableConfigAsync(ulong guildId, GuildConfigType type, bool shouldSave = false);
        Task<Guild> EnableConfigAsync(ulong guildId, GuildConfigType type, bool shouldSave = false);
        Task RepairModuleConfigAsync(TicketingConfigRepairReqDto req, bool shouldSave = false);
        Task RepairModuleConfigAsync(ModerationConfigRepairReqDto req, bool shouldSave = false);
        Task EditTicketingConfigAsync(TicketingConfigEditReqDto req, bool shouldSave = false);
        Task EditModerationConfigAsync(ulong guildId, bool shouldSave = false);
    }
}