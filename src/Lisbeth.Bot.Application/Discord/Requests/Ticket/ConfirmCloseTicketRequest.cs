using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.HandlerServices;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class ConfirmCloseTicketRequest : HandlerRequestBase
{
    public ConfirmCloseTicketRequest(TicketCloseReqDto dto, DiscordInteraction? interaction = null)
    {
        Interaction = interaction;
        Dto = dto;
    }

    public DiscordInteraction? Interaction { get; set; }
    public TicketCloseReqDto Dto { get; set; }
}
