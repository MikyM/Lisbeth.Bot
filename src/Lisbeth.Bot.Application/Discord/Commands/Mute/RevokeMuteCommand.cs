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
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using MikyM.CommandHandlers.Commands;

namespace Lisbeth.Bot.Application.Discord.Commands.Mute;

public class RevokeMuteCommand : CommandBase<DiscordEmbed>
{
    public RevokeMuteCommand(MuteRevokeReqDto dto)
    {
        Dto = dto;
    }
    public RevokeMuteCommand(MuteRevokeReqDto dto, InteractionContext ctx)
    {
        Dto = dto;
        Ctx = ctx;
    }
    public RevokeMuteCommand(MuteRevokeReqDto dto, ContextMenuContext ctx)
    {
        Dto = dto;
        MenuCtx = ctx;
    }
    public MuteRevokeReqDto Dto { get; set; }
    public InteractionContext? Ctx { get; set; }
    public ContextMenuContext? MenuCtx { get; set; }
}
