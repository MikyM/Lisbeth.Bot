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
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;

namespace Lisbeth.Bot.Application.Validation.RoleMenu;

public class RoleMenuEditReqValidator : AbstractValidator<RoleMenuEditReqDto>
{
    public RoleMenuEditReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public RoleMenuEditReqValidator(DiscordClient discord)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id).NotEmpty().When(x => x.Name is not null && !x.GuildId.HasValue);
        RuleFor(x => x.Name).NotEmpty().When(x => !x.Id.HasValue);
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .When(x => !x.Id.HasValue)
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<RoleMenuEditReqDto>(discord)));
        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<RoleMenuEditReqDto>(discord)));
        RuleFor(x => x.Text).NotEmpty().When(x => x.EmbedConfig is null);
        RuleFor(x => x.EmbedConfig).NotEmpty().When(x => x.Text is null or "");
    }
}
