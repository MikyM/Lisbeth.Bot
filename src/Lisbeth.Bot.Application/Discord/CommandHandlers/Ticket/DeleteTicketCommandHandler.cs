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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class DeleteTicketCommandHandler : ICommandHandler<DeleteTicketCommand>
{
    public async Task<Result> HandleAsync(DeleteTicketCommand command)
    {
        await command.Interaction.CreateFollowupMessageAsync(
            new DiscordFollowupMessageBuilder().AddEmbed(
                new DiscordEmbedBuilder().WithDescription("This ticket will be deleted in 5 seconds")));

        await Task.Delay(5000);

        await command.Interaction.Channel.DeleteAsync();

        return Result.FromSuccess();
    }
}
