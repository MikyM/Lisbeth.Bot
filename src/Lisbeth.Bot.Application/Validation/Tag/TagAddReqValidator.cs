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

using DSharpPlus;
using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation.Tag;

public class TagAddReqValidator : AbstractValidator<TagAddReqDto>
{
    public TagAddReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public TagAddReqValidator(DiscordClient discord)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
            .DependentRules(x =>
                x.Must(value => !(value ?? " ").All(char.IsWhiteSpace)).WithMessage("Tag names can't have spaces"));
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<TagAddReqDto>(discord)));
        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<TagAddReqDto>(discord)));
        RuleFor(x => x.Text).NotEmpty().When(x => x.EmbedConfig is null);
        RuleFor(x => x.EmbedConfig).NotEmpty().When(x => string.IsNullOrWhiteSpace(x.Text));
    }
}