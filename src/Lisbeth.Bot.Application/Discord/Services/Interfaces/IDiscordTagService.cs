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
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordTagService
    {
        Task<DiscordEmbed> AddAsync(TagAddReqDto req);
        Task<DiscordEmbed> AddAsync(InteractionContext ctx, TagAddReqDto req);
        Task<DiscordEmbed> EditAsync(TagEditReqDto req);
        Task<DiscordEmbed> EditAsync(InteractionContext ctx, TagEditReqDto req);
        Task<(DiscordEmbed Embed, string Text)> GetAsync(TagGetReqDto req);
        Task<(DiscordEmbed Embed, string Text)> GetAsync(InteractionContext ctx, TagGetReqDto req);
        Task<DiscordEmbed> DisableAsync(TagDisableReqDto req);
        Task<DiscordEmbed> DisableAsync(InteractionContext ctx, TagDisableReqDto req);
    }
}