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
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.Application.Validation.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordCleanClosedTicketsHandler : IDiscordCleanClosedTicketsHandler
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordCleanClosedTicketsHandler> _logger;
    private readonly IDiscordConfirmCloseTicketHandler _closeTicketHandler;

    public DiscordCleanClosedTicketsHandler(IDiscordService discord, IGuildDataService guildDataService,
        ILogger<DiscordCleanClosedTicketsHandler> logger, IDiscordConfirmCloseTicketHandler closeTicketHandler)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _closeTicketHandler = closeTicketHandler;
    }

    public async Task<Result> HandleAsync(CleanClosedTicketsRequest request)
    {
        try
        {
            foreach (var (guildId, guild) in _discord.Client.Guilds)
            {
                var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingAndTicketsSpecifications(guildId));

                if (!res.IsDefined(out var guildCfg)) continue;

                if (guildCfg.TicketingConfig?.CloseAfter is null) continue;
                if (guildCfg.Tickets?.Count == 0) continue;

                DiscordChannel openedCat;
                try
                {
                    openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing opened ticket category set with Id: {guildCfg.TicketingConfig.OpenedCategoryId}.");
                    continue;
                }

                foreach (var openedTicketChannel in openedCat.Children)
                {
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != openedTicketChannel.Id)) continue;

                    var lastMessage = await openedTicketChannel.GetMessagesAsync(1);
                    var msg = lastMessage?.FirstOrDefault();
                    if (msg is null) continue;

                    if (!((DiscordMember)msg.Author).IsModerator()) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(msg.Timestamp.UtcDateTime);

                    var req = new TicketCloseReqDto(null, guildId, openedTicketChannel.Id,
                        _discord.Client.CurrentUser.Id);

                    var validator = new TicketCloseReqValidator(_discord);
                    await validator.ValidateAndThrowAsync(req);

                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CloseAfter.Value.Hours)
                        await _closeTicketHandler.HandleAsync(new ConfirmCloseTicketRequest(req));

                    await Task.Delay(500);
                }

                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with closing inactive tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }
}