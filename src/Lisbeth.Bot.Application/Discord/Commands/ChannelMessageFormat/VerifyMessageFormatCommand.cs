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

using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.ChannelMessageFormat;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;

public class VerifyMessageFormatCommand : CommandBase<VerifyMessageFormatResDto>
{
    public MessageCreateEventArgs? EventArgs { get; set; }
    public InteractionContext? Ctx { get; set; }
    public VerifyMessageFormatReqDto Dto { get; set; }

    public VerifyMessageFormatCommand(VerifyMessageFormatReqDto dto, MessageCreateEventArgs? eventArgs = null)
    {
        Dto = dto;
        EventArgs = eventArgs;
    }

    public VerifyMessageFormatCommand(VerifyMessageFormatReqDto dto, InteractionContext? ctx = null)
    {
        Dto = dto;
        Ctx = ctx;
    }

    public VerifyMessageFormatCommand(VerifyMessageFormatReqDto dto)
    {
        Dto = dto;
    }
}