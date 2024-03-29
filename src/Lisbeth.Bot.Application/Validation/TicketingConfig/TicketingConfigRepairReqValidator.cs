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

using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

namespace Lisbeth.Bot.Application.Validation.TicketingConfig;

public class TicketingConfigRepairReqValidator : AbstractValidator<TicketingConfigRepairReqDto>
{
    public TicketingConfigRepairReqValidator(IDiscordService discord) : this(discord.Client)
    {
    }

    public TicketingConfigRepairReqValidator(DiscordClient client)
    {
        RuleFor(x => x.GuildId)
            .SetAsyncValidator(new DiscordGuildIdValidator<TicketingConfigRepairReqDto>(client));
        RuleFor(x => x.RequestedOnBehalfOfId)
            .SetAsyncValidator(new DiscordUserIdValidator<TicketingConfigRepairReqDto>(client));
    }
}
