using System;
using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation
{
    public class BanGetReqValidator : AbstractValidator<BanGetReqDto>
    {
        public BanGetReqValidator(IDiscordService discord)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id).NotEmpty().When(x => !x.GuildId.HasValue || !x.TargetUserId.HasValue);

            RuleFor(x => x.GuildId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && x.TargetUserId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<BanGetReqDto>(discord)));
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .When(x => x.Id.HasValue && x.GuildId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord)));

            RuleFor(x => x.RequestedOnBehalfOfId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord)));

            RuleFor(x => x.AppliedById)
                .SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord, true))
                .When(x => x.AppliedById.HasValue);
            RuleFor(x => x.LiftedById)
                .SetAsyncValidator(new DiscordUserIdValidator<BanGetReqDto>(discord, true))
                .When(x => x.LiftedById.HasValue);

            // ReSharper disable once PossibleInvalidOperationException
            RuleFor(x => x.AppliedOn).Must(x => x.Value <= DateTime.UtcNow).When(x => x.AppliedOn.HasValue);
            RuleFor(x => x.LiftedOn).Must(x => x.Value <= DateTime.UtcNow).When(x => x.LiftedOn.HasValue);
        }
    }
}
