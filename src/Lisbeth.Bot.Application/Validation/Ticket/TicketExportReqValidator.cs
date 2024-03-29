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
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;

namespace Lisbeth.Bot.Application.Validation.Ticket;

public class TicketExportReqValidator : AbstractValidator<TicketExportReqDto>
{
    public TicketExportReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public TicketExportReqValidator(DiscordClient discord)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.GuildId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<TicketExportReqDto>(discord)));
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .When(x => !x.OwnerId.HasValue)
            .DependentRules(x => x.SetAsyncValidator(new DiscordChannelIdValidator<TicketExportReqDto>(discord)));
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .When(x => !x.ChannelId.HasValue)
            .DependentRules(x => x.SetAsyncValidator(new DiscordChannelIdValidator<TicketExportReqDto>(discord)));

        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<TicketExportReqDto>(discord)));
    }
}
