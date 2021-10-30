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
using Lisbeth.Bot.Domain.DTOs.Request.Mute;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordMuteService
    {
        Task<DiscordEmbed> MuteAsync(MuteReqDto req);
        Task<DiscordEmbed> MuteAsync(ContextMenuContext ctx, MuteReqDto req);
        Task<DiscordEmbed> MuteAsync(InteractionContext ctx, MuteReqDto req);
        Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req);
        Task<DiscordEmbed> UnmuteAsync(ContextMenuContext ctx, MuteDisableReqDto req);
        Task<DiscordEmbed> UnmuteAsync(InteractionContext ctx, MuteDisableReqDto req);
        Task<DiscordEmbed> GetSpecificUserGuildMuteAsync(MuteGetReqDto req);
        Task<DiscordEmbed> GetSpecificUserGuildMuteAsync(InteractionContext ctx, MuteGetReqDto req);
        Task<DiscordEmbed> GetSpecificUserGuildMuteAsync(ContextMenuContext ctx, MuteGetReqDto req);
        Task UnmuteCheckAsync();
    }
}