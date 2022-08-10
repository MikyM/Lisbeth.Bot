using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;

namespace Lisbeth.Bot.Application.Discord.Commands.Reminder;

public class DisableReminderCommand : CommandBase<DiscordEmbed>
{
    public DisableReminderCommand(DisableReminderReqDto dto, InteractionContext? ctx = null)
    {
        Dto = dto;
        Ctx = ctx;
    }

    public DisableReminderReqDto Dto { get; set; }
    public InteractionContext? Ctx { get; set; }
}
