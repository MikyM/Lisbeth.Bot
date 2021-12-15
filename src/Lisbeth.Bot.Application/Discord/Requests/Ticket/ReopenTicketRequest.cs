﻿using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using MikyM.Common.Application.HandlerServices;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class ReopenTicketRequest : HandlerRequestBase
{
    public ReopenTicketRequest(TicketReopenReqDto dto, DiscordInteraction? interaction = null)
    {
        Interaction = interaction;
        Dto = dto;
    }

    public DiscordInteraction? Interaction { get; set; }
    public TicketReopenReqDto Dto { get; set; }
}
