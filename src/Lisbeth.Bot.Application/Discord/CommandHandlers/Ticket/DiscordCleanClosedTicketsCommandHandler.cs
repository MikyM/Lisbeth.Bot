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
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class DiscordCleanClosedTicketsCommandHandler : ICommandHandler<CleanClosedTicketsCommand>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordCleanClosedTicketsCommandHandler> _logger;

    public DiscordCleanClosedTicketsCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        ILogger<DiscordCleanClosedTicketsCommandHandler> logger)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CleanClosedTicketsCommand command)
    {
        try
        {
            foreach (var (guildId, _) in _discord.Client.Guilds)
            {
                var res = await _guildDataService.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithTicketingAndInactiveTicketsSpecifications(guildId));

                if (!res.IsDefined(out var guildCfg)) continue;

                if (guildCfg.TicketingConfig?.CleanAfter is null) continue;
                if (guildCfg.Tickets?.Count == 0) continue;

                DiscordChannel closedCat;
                try
                {
                    closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing closed ticket category set with Id: {guildCfg.TicketingConfig.ClosedCategoryId}.");
                    continue;
                }

                foreach (var closedTicketChannel in closedCat.Children)
                {
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != closedTicketChannel.Id)) continue;

                    var lastMessage = await closedTicketChannel.GetMessagesAsync(1);
                    if (lastMessage is null || lastMessage.Count == 0) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(lastMessage[0].Timestamp.UtcDateTime);
                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CleanAfter.Value.Hours)
                    {
                        await closedTicketChannel.DeleteAsync();
                        _logger.LogDebug($"Deleting channel Id: {closedTicketChannel.Id} with name: {closedTicketChannel.Name}");
                    }
                    await Task.Delay(500);
                }

                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with cleaning closed tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }
}
