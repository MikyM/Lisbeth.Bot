using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Application.CommandHandlers.Commands;

namespace Lisbeth.Bot.Application.Discord.Commands.Reminder;

public class SetNewReminderCommand : CommandBase<DiscordEmbed>
{
    public SetNewReminderCommand(SetReminderReqDto dto, InteractionContext? ctx = null)
    {
        Ctx = ctx;
        Dto = dto;
    }

    public InteractionContext? Ctx { get; set; }
    public SetReminderReqDto Dto { get; set; }
}
