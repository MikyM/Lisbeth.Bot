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
using Lisbeth.Bot.Domain.DTOs.Request.Ban;

namespace Lisbeth.Bot.Application.Validation.Ban;

public class BanGetReqValidator : AbstractValidator<BanGetReqDto>
{
    public BanGetReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public BanGetReqValidator(DiscordClient discord)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id).NotEmpty().When(x => !x.TargetUserId.HasValue);

        RuleFor(x => x.GuildId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<BanGetReqDto>(discord)));
        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .When(x => !x.Id.HasValue)
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord, true)));

        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord)));

        RuleFor(x => x.AppliedById)
            .SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord, true))
            .When(x => x.AppliedById.HasValue);
        RuleFor(x => x.LiftedById)
            .SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord, true))
            .When(x => x.LiftedById.HasValue);

        RuleFor(x => x.AppliedOn).Must(x => x!.Value <= DateTime.UtcNow).When(x => x.AppliedOn.HasValue);
        RuleFor(x => x.LiftedOn).Must(x => x!.Value <= DateTime.UtcNow).When(x => x.LiftedOn.HasValue);
    }
}
