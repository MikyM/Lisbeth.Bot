using Lisbeth.Bot.Domain.DTOs.Request.Reminder;

namespace Lisbeth.Bot.Application.Discord.Commands.Reminder;

public class RescheduleReminderCommand : ICommand<DiscordEmbed>
{
    public RescheduleReminderCommand(RescheduleReminderReqDto dto, InteractionContext? ctx = null)
    {
        Ctx = ctx;
        Dto = dto;
    }

    public InteractionContext? Ctx { get; set; }
    public RescheduleReminderReqDto Dto { get; set; }
}
