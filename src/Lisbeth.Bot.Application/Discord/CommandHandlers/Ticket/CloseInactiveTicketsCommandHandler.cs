﻿// This file is part of Lisbeth.Bot project
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

using System.Collections.Generic;
using DSharpPlus.Entities;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.Application.Validation.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class CloseInactiveTicketsCommandHandler : ICommandHandler<CloseInactiveTicketsCommand>
{
    private readonly IDiscordService _discord;
    private readonly IGuildService _guildService;
    private readonly ILogger<CloseInactiveTicketsCommandHandler> _logger;
    private readonly ICommandHandler<ConfirmCloseTicketCommand> _closeTicketCommandHandler;

    public CloseInactiveTicketsCommandHandler(IDiscordService discord, IGuildService guildService,
        ILogger<CloseInactiveTicketsCommandHandler> logger, ICommandHandler<ConfirmCloseTicketCommand> closeTicketCommandHandler)
    {
        _discord = discord;
        _guildService = guildService;
        _logger = logger;
        _closeTicketCommandHandler = closeTicketCommandHandler;
    }

    public async Task<Result> HandleAsync(CloseInactiveTicketsCommand command)
    {
        try
        {
            await Parallel.ForEachAsync(_discord.Client.Guilds.Keys, async (guildId, _) =>
            {
                var res = await _guildService.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithTicketingAndTicketsSpecifications(guildId));

                if (!res.IsDefined(out var guildCfg)) return;

                if (guildCfg.TicketingConfig?.CloseAfter is null) return;
                if (guildCfg.Tickets?.Count() == 0) return;

                DiscordChannel openedCat;
                try
                {
                    openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing opened ticket category set with Id: {guildCfg.TicketingConfig.OpenedCategoryId}.");
                    return;
                }

                if (openedCat is null) return;

                foreach (var openedTicketChannel in openedCat.Children)
                {
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != openedTicketChannel.Id)) continue;

                    IReadOnlyList<DiscordMessage> lastMessages;
                    try
                    {
                        lastMessages = await openedTicketChannel.GetMessagesAsync(1);
                    }
                    catch
                    {
                        continue;
                    }

                    var msg = lastMessages?.FirstOrDefault();
                    if (msg is null) continue;

                    if (msg.Author is DiscordMember member && !member.IsModerator()) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(msg.Timestamp.UtcDateTime);

                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CloseAfter.Value.TotalHours)
                    {
                        var req = new TicketCloseReqDto(null, guildId, openedTicketChannel.Id,
                            _discord.Client.CurrentUser.Id);

                        var validator = new TicketCloseReqValidator(_discord);
                        await validator.ValidateAndThrowAsync(req);

                        await _closeTicketCommandHandler.HandleAsync(new ConfirmCloseTicketCommand(req));

                        _logger.LogDebug($"Closing channel Id: {openedTicketChannel.Id} with name: {openedTicketChannel.Name}");
                    }

                    await Task.Delay(500);
                }

                await Task.Delay(500);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with closing inactive tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }
}