using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation
{
    public class TicketAddReqValidator : AbstractValidator<TicketAddReqDto>
    {
        public TicketAddReqValidator(IDiscordService discord)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty()
                .When(x => !x.GuildId.HasValue || !x.ChannelId.HasValue || !x.GuildSpecificId.HasValue ||
                           !x.OwnerId.HasValue);
            RuleFor(x => x.GuildId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && !x.ChannelId.HasValue &&
                           (x.GuildSpecificId.HasValue || x.OwnerId.HasValue))
                .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<TicketAddReqDto>(discord)));
            RuleFor(x => x.ChannelId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && !x.GuildId.HasValue && x.GuildSpecificId.HasValue && x.OwnerId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordChannelIdValidator<TicketAddReqDto>(discord)));
            RuleFor(x => x.OwnerId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && x.GuildId.HasValue && !x.GuildSpecificId.HasValue && !x.ChannelId.HasValue)
                .DependentRules(x => x.SetAsyncValidator(new DiscordChannelIdValidator<TicketAddReqDto>(discord)));
            RuleFor(x => x.GuildSpecificId)
                .NotEmpty()
                .When(x => !x.Id.HasValue && x.GuildId.HasValue && !x.GuildSpecificId.HasValue &&
                           !x.ChannelId.HasValue);

            RuleFor(x => x.RequestedById)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<TicketAddReqDto>(discord)));

            RuleFor(x => x.SnowflakeId)
                .NotEmpty()
                .DependentRules(x => x.SetAsyncValidator(new DiscordSnowflakeIdValidator<TicketAddReqDto>(discord)));
        }
    }
}
