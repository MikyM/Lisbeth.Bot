using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Commands.Reminder;

public class RescheduleReminderCommand : CommandBase<DiscordEmbed>
{
    public RescheduleReminderCommand(RescheduleReminderReqDto dto, InteractionContext? ctx = null)
    {
        Ctx = ctx;
        Dto = dto;
    }

    public InteractionContext? Ctx { get; set; }
    public RescheduleReminderReqDto Dto { get; set; }
}
