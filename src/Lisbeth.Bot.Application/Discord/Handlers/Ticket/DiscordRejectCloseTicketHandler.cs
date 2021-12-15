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

using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

public class DiscordRejectCloseTicketHandler : IDiscordRejectCloseTicketHandler
{
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordCloseTicketHandler> _logger;

    public DiscordRejectCloseTicketHandler(IGuildDataService guildDataService,
        ILogger<DiscordCloseTicketHandler> logger)
    {
        _guildDataService = guildDataService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(RejectCloseTicketRequest request)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(request.Interaction.Guild.Id));

        if (!guildRes.IsDefined(out var guildCfg)) return Result.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{request.Interaction.Guild.Id} doesn't have ticketing enabled.");

        await request.Interaction.Channel.DeleteMessageAsync(request.Message);

        return Result.FromSuccess();
    }
}
