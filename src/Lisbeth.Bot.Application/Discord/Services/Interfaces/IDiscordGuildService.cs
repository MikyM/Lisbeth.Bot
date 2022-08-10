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

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ReminderConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordGuildService
{
    Task<Result> HandleGuildCreateAsync(GuildCreateEventArgs args);
    Task<Result> HandleGuildDeleteAsync(GuildDeleteEventArgs args);
    Task<Result<DiscordEmbed>> CreateModuleAsync(ModerationConfigReqDto req);
    Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, ModerationConfigReqDto req);
    Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, TicketingConfigReqDto req);
    Task<Result<DiscordEmbed>> CreateModuleAsync(TicketingConfigReqDto req);
    Task<Result<DiscordEmbed>> CreateModuleAsync(ReminderConfigReqDto req);
    Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, ReminderConfigReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(ModerationConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(TicketingConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(ReminderConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx, ReminderConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx, ModerationConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx, TicketingConfigDisableReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(ModerationConfigRepairReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(TicketingConfigRepairReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(ReminderConfigRepairReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx, ReminderConfigRepairReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx, ModerationConfigRepairReqDto req);
    Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx, TicketingConfigRepairReqDto req);
    Task<Result<int>> CreateOverwritesForMutedRoleAsync(CreateMuteOverwritesReqDto req);
    Task<Result<int>> CreateOverwritesForMutedRoleAsync(InteractionContext ctx, CreateMuteOverwritesReqDto req);
    Task<Result> PrepareBotAsync(IEnumerable<ulong> guildIds);
    Task<Result> SetPhishingDetectionAsync(SetPhishingReqDto req);
}
