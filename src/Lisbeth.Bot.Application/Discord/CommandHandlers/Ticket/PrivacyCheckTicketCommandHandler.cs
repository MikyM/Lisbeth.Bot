// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSharpPlus;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class PrivacyCheckTicketCommandHandler : ICommandHandler<PrivacyCheckTicketCommand, bool>
{
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

