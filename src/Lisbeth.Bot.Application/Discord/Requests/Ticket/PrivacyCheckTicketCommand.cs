using DSharpPlus.Entities;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Requests.Ticket;

public class PrivacyCheckTicketCommand : ICommand
{
    public PrivacyCheckTicketCommand(DiscordGuild guild, Domain.Entities.Ticket ticket)
    {
        
        Guild = guild;
        Ticket = ticket;
    }

    public DiscordGuild Guild { get; set; }
    public Domain.Entities.Ticket Ticket { get; set; }
}
