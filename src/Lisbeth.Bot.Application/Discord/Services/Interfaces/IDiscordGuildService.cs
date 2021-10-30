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
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordGuildService
    {
        Task HandleGuildCreateAsync(GuildCreateEventArgs args);
        Task HandleGuildDeleteAsync(GuildDeleteEventArgs args);
        Task<DiscordEmbed> CreateModuleAsync(ModerationConfigReqDto req);
        Task<DiscordEmbed> CreateModuleAsync(InteractionContext ctx, ModerationConfigReqDto req);
        Task<DiscordEmbed> CreateModuleAsync(InteractionContext ctx, TicketingConfigReqDto req);
        Task<DiscordEmbed> CreateModuleAsync(TicketingConfigReqDto req);
        Task<DiscordEmbed> DisableModuleAsync(ModerationConfigDisableReqDto req);
        Task<DiscordEmbed> DisableModuleAsync(TicketingConfigDisableReqDto req);
        Task<DiscordEmbed> DisableModuleAsync(ModerationConfigDisableReqDto req, InteractionContext ctx);
        Task<DiscordEmbed> DisableModuleAsync(TicketingConfigDisableReqDto req, InteractionContext ctx);
        Task<DiscordEmbed> RepairConfigAsync(ModerationConfigRepairReqDto req);
        Task<DiscordEmbed> RepairConfigAsync(TicketingConfigRepairReqDto req);
        Task<DiscordEmbed> RepairConfigAsync(ModerationConfigRepairReqDto req, InteractionContext ctx);
        Task<DiscordEmbed> RepairConfigAsync(TicketingConfigRepairReqDto req, InteractionContext ctx);
    }
}