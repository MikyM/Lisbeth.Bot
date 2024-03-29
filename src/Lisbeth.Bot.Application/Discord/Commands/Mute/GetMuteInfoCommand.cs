﻿// This file is part of Lisbeth.Bot project
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

using Lisbeth.Bot.Domain.DTOs.Request.Mute;

namespace Lisbeth.Bot.Application.Discord.Commands.Mute;

public class GetMuteInfoCommand : ICommand<DiscordEmbed>
{
    public GetMuteInfoCommand(MuteGetReqDto dto)
    {
        Dto = dto;
    }

    public GetMuteInfoCommand(MuteGetReqDto dto, InteractionContext ctx)
    {
        Dto = dto;
        Ctx = ctx;
    }

    public GetMuteInfoCommand(MuteGetReqDto dto, ContextMenuContext ctx)
    {
        Dto = dto;
        MenuCtx = ctx;
    }

    public MuteGetReqDto Dto { get; set; }
    public InteractionContext? Ctx { get; set; }
    public ContextMenuContext? MenuCtx { get; set; }
}
