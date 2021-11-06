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
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using MikyM.Common.Application.Results;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordMuteService
    {
        Task<Result<DiscordEmbed>> MuteAsync(MuteReqDto req);
        Task<Result<DiscordEmbed>> MuteAsync(ContextMenuContext ctx, MuteReqDto req);
        Task<Result<DiscordEmbed>> MuteAsync(InteractionContext ctx, MuteReqDto req);
        Task<Result<DiscordEmbed>> UnmuteAsync(MuteDisableReqDto req);
        Task<Result<DiscordEmbed>> UnmuteAsync(ContextMenuContext ctx, MuteDisableReqDto req);
        Task<Result<DiscordEmbed>> UnmuteAsync(InteractionContext ctx, MuteDisableReqDto req);
        Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(MuteGetReqDto req);
        Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(InteractionContext ctx, MuteGetReqDto req);
        Task<Result<DiscordEmbed>> GetSpecificUserGuildMuteAsync(ContextMenuContext ctx, MuteGetReqDto req);
        Task<Result> UnmuteCheckAsync();
    }
}