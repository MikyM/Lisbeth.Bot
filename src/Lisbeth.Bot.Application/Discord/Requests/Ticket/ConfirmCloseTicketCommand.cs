using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class ConfirmCloseTicketCommand : CommandBase
{
    public ConfirmCloseTicketCommand(TicketCloseReqDto dto, DiscordInteraction? interaction = null)
    {
        Interaction = interaction;
        Dto = dto;
    }

    public DiscordInteraction? Interaction { get; set; }
    public TicketCloseReqDto Dto { get; set; }
}
