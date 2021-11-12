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
using Lisbeth.Bot.Domain.DTOs.Request.Tag;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordTagService
{
    Task<Result<DiscordEmbed>> AddAsync(TagAddReqDto req);
    Task<Result<DiscordEmbed>> AddAsync(InteractionContext ctx, TagAddReqDto req);
    Task<Result<DiscordEmbed>> EditAsync(TagEditReqDto req);
    Task<Result<DiscordEmbed>> EditAsync(InteractionContext ctx, TagEditReqDto req);
    Task<Result<(DiscordEmbed? Embed, string Text)>> GetAsync(TagGetReqDto req);
    Task<Result<(DiscordEmbed? Embed, string Text)>> GetAsync(InteractionContext ctx, TagGetReqDto req);
    Task<Result<(DiscordEmbed? Embed, string Text)>> SendAsync(TagSendReqDto req);
    Task<Result<(DiscordEmbed? Embed, string Text)>> SendAsync(InteractionContext ctx, TagSendReqDto req);
    Task<Result<DiscordEmbed>> DisableAsync(TagDisableReqDto req);
    Task<Result<DiscordEmbed>> DisableAsync(InteractionContext ctx, TagDisableReqDto req);
}