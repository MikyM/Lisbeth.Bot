using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class OpenTicketCommand : ICommand
{
    public OpenTicketCommand(TicketOpenReqDto dto, DiscordInteraction? interaction = null)
    {
        Dto = dto;
        Interaction = interaction;
    }

    public TicketOpenReqDto Dto { get; set; }
    public DiscordInteraction? Interaction { get; set; }
}
