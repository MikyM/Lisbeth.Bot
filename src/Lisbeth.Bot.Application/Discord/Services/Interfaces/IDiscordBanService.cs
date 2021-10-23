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

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordBanService
    {
        Task<DiscordEmbed> BanAsync(BanReqDto req);
        Task<DiscordEmbed> BanAsync(InteractionContext ctx, BanReqDto req);
        Task<DiscordEmbed> UnbanAsync(BanDisableReqDto req);
        Task<DiscordEmbed> UnbanAsync(InteractionContext ctx, BanDisableReqDto req);
        Task<DiscordEmbed> GetSpecificUserGuildBanAsync(BanGetReqDto req);
        Task<DiscordEmbed> GetSpecificUserGuildBanAsync(InteractionContext ctx, BanGetReqDto req);
        Task UnbanCheckAsync();
    }
}