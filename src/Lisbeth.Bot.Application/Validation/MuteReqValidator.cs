using System;
using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation
{
    public class MuteReqValidator : AbstractValidator<MuteReqDto>
    {
        public MuteReqValidator(IDiscordService discord)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.GuildId).NotEmpty();
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<MuteReqDto>(discord)));
            RuleFor(x => x.RequestedOnBehalfOfId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<MuteReqDto>(discord)));
            RuleFor(x => x.AppliedUntil).NotEmpty().Must(x => x.ToUniversalTime() > DateTime.UtcNow);
        }
    }
}
