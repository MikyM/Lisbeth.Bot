// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using System.Collections.Generic;
using Autofac;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class CleanClosedTicketsCommandHandler : IAsyncCommandHandler<CleanClosedTicketsCommand>
{
    private readonly IDiscordService _discord;
    private readonly ILogger<CleanClosedTicketsCommandHandler> _logger;
    private readonly ILifetimeScope _scope;

    public CleanClosedTicketsCommandHandler(IDiscordService discord, ILifetimeScope scope,
        ILogger<CleanClosedTicketsCommandHandler> logger)
    {
        _discord = discord;
        _scope = scope;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CleanClosedTicketsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await Parallel.ForEachAsync(_discord.Client.Guilds.Keys, async (guildId, _) =>
            {
                await using var scope = _scope.BeginLifetimeScope();
                var res = await scope.Resolve<IGuildDataService>().GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithTicketingAndInactiveTicketsSpecifications(guildId));

                if (!res.IsDefined(out var guildCfg)) return;

                if (guildCfg.TicketingConfig?.CleanAfter is null) return;
                if (guildCfg.Tickets?.Count() == 0) return;

                DiscordChannel closedCat;
                try
                {
                    closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing closed ticket category set with Id: {guildCfg.TicketingConfig.ClosedCategoryId}.");
                    return;
                }

                if (closedCat is null) return;

                foreach (var closedTicketChannel in closedCat.Children)
                {
                    if (closedTicketChannel.Id == guildCfg.TicketingConfig.LogChannelId)
                        continue;
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != closedTicketChannel.Id)) continue;

                    IList<DiscordMessage> lastMessages;
                    try
                    {
                        lastMessages = new List<DiscordMessage>();

                        await foreach(var message in closedTicketChannel.GetMessagesAsync(1))
                            lastMessages.Add(message);
                    }
                    catch
                    {
                        continue;
                    }

                    if (lastMessages is null || lastMessages.Count == 0) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(lastMessages[0].Timestamp.UtcDateTime);
                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CleanAfter.Value.TotalHours)
                    {
                        try
                        {
                            await closedTicketChannel.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to delete channel cause: {ex}");
                        }
                        _logger.LogDebug($"Deleting channel Id: {closedTicketChannel.Id} with name: {closedTicketChannel.Name}");
                    }
                    await Task.Delay(500);
                }

                await Task.Delay(500);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with cleaning closed tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }
}
