using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;
using MikyM.Discord.Services;

namespace Lisbeth.Bot.Application.Validation
{
    public class TicketOpenReqValidator : AbstractValidator<TicketOpenReqDto>
    {
        public TicketOpenReqValidator(IDiscordService discord)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.GuildId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<TicketOpenReqDto>(discord)));
            RuleFor(x => x.OwnerId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<TicketOpenReqDto>(discord)));
        }
    }
}
