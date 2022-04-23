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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordGuildRequestDataProvider
{
    DiscordGuild DiscordGuild { get; }
    DiscordMember RequestingMember { get; }

    InteractionContext? InteractionContext { get; set; }
    DiscordInteraction? DiscordInteraction { get; set; }
    bool IsInitialized { get; }

    Task<Result<DiscordMember>> GetMemberAsync(ulong userId);
    Task<Result<DiscordMember>> GetFirstResolvedMemberOrAsync(ulong userId);

    Task<Result<DiscordRole>> GetRoleAsync(ulong roleId);
    Task<Result<DiscordRole>> GetFirstResolvedRoleOrAsync(ulong roleId);

    Task<Result<DiscordChannel>> GetChannelAsync(ulong channelId);
    Task<Result<DiscordChannel>> GetFirstResolvedChannelOrAsync(ulong channelId);

    Task<Result<(SnowflakeObject Snowflake, DiscordEntity Type)>> GetFirstResolvedSnowflakeOrAsync(ulong id, DiscordEntity? type = null);

    Task<Result<(SnowflakeObject Snowflake, DiscordEntity Type)>> GetFirstResolvedRoleOrMemberOrAsync(ulong id);

    Task<Result<DiscordMember>> GetOwnerAsync();

    Task<Result> InitializeAsync(IBaseModAuthReq baseDto);
    Task<Result> InitializeAsync(IBaseModAuthReq baseDto, InteractionContext? ctx);
    Task<Result> InitializeAsync(IBaseModAuthReq baseDto, DiscordInteraction? interaction);
}