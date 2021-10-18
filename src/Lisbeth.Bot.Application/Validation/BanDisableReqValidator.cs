using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation
{
    public class BanDisableReqValidator : AbstractValidator<BanDisableReqDto>
    {
        public BanDisableReqValidator(IDiscordService discord)
        {
            CascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Id).NotEmpty().When(x => !x.GuildId.HasValue || !x.TargetUserId.HasValue);

            RuleFor(x => x.GuildId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && x.TargetUserId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<BanDisableReqDto>(discord)));
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .When(x => x.Id.HasValue && x.GuildId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanDisableReqDto>(discord)));

            RuleFor(x => x.RequestedOnBehalfOfId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<BanDisableReqDto>(discord)));
        }
    }
}
