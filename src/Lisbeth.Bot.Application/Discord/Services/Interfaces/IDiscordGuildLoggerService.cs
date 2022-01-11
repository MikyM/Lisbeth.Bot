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
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordGuildLoggerService
{
    Task<Result> LogToDiscordAsync<TRequest>(DiscordGuild discordGuild, TRequest req,
        DiscordModeration moderation, DiscordUser? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null)
        where TRequest : class, IBaseModAuthReq;

    Task<Result> LogToDiscordAsync<TRequest>(Guild guild, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null,
        SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq;

    Task<Result> LogToDiscordAsync<TRequest>(ulong discordGuildId, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null,
        SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq;

    Task<Result> LogToDiscordAsync<TRequest>(long guildId, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null,
        SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq;
}