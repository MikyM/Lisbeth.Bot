using DSharpPlus;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class DiscordPrivacyCheckTicketCommandHandler : ICommandHandler<PrivacyCheckTicketCommand, bool>
{
    private readonly IDiscordService _discord;

    public DiscordPrivacyCheckTicketCommandHandler(IDiscordService discord)
    {
        _discord = discord;
    }

    public async Task<Result<bool>> HandleAsync(PrivacyCheckTicketCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        if (command.Ticket.AddedUserIds?.Count == 0) return new InvalidOperationError("User list was empty");

        if (command.Ticket.AddedUserIds is null) return true;

        foreach (var id in command.Ticket.AddedUserIds)
        {
            try
            {
                var member = await command.Guild.GetMemberAsync(id);
                if (!member.Permissions.HasPermission(Permissions.Administrator) && member.Id != command.Ticket.UserId)
                    return false;
            }
            catch (Exception)
            {
                continue;
            }

            await Task.Delay(500);
        }

        return true;
    }
}

