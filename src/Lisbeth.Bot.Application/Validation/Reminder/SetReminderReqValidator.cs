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
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Validation.Reminder;

public class SetReminderReqValidator : AbstractValidator<SetReminderReqDto>
{
    public SetReminderReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public SetReminderReqValidator(DiscordClient discord)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<SetReminderReqDto>(discord)));
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<SetReminderReqDto>(discord)));
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.CronExpression)
            .NotEmpty()
            .When(x => x.SetFor is null && string.IsNullOrWhiteSpace(x.TimeSpanExpression));
        RuleFor(x => x.SetFor)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.TimeSpanExpression) &&
                       string.IsNullOrWhiteSpace(x.CronExpression))
            .InclusiveBetween(DateTime.UtcNow, DateTime.UtcNow.AddYears(1))
            .When(x => x.SetFor.HasValue);
        RuleFor(x => x.TimeSpanExpression)
            .NotEmpty()
            .When(x => x.SetFor is null && string.IsNullOrWhiteSpace(x.CronExpression))
            .Must(y => y!.TryParseToDurationAndNextOccurrence(out _, out _))
            .When(x => !string.IsNullOrEmpty(x.TimeSpanExpression));
        RuleFor(x => x.Mentions)
            .NotEmpty()
            .DependentRules(x => x.ForEach(y => y.NotEmpty().Must(m => m.TryParseDiscordMention(out _))));
        RuleFor(x => x.Text).NotEmpty();
        RuleFor(x => x.ChannelId).SetAsyncValidator(new DiscordChannelIdValidator<SetReminderReqDto>(discord))
            .When(x => x.ChannelId.HasValue);
    }
}