using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.HandlerServices;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class OpenTicketRequest : IHandlerRequest
{
    public OpenTicketRequest(TicketOpenReqDto dto, DiscordInteraction? interaction = null)
    {
        Dto = dto;
        Interaction = interaction;
    }

    public TicketOpenReqDto Dto { get; set; }
    public DiscordInteraction? Interaction { get; set; }
}
