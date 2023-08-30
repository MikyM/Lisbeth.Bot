using Lisbeth.Bot.Domain.DTOs.Request.Reminder;

namespace Lisbeth.Bot.Application.Discord.Commands.Reminder;

public class SetNewReminderCommand : ICommand<DiscordEmbed>
{
    public SetNewReminderCommand(SetReminderReqDto dto, InteractionContext? ctx = null)
    {
        Ctx = ctx;
        Dto = dto;
    }

    public InteractionContext? Ctx { get; set; }
    public SetReminderReqDto Dto { get; set; }
}
