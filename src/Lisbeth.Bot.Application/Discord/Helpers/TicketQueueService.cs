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

using Autofac;
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities;
using System.Collections.Concurrent;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Helpers;

public interface ITicketQueueService
{
    Task EnqueueAsync(TicketOpenReqDto req);
    Task EnqueueAsync(TicketOpenReqDto req, DiscordInteraction interaction);
    bool AddGuildQueue(ulong guildId);
    bool RemoveGuildQueue(ulong guildId);
}

public class TicketQueueService : ITicketQueueService
{
    private readonly ConcurrentDictionary<ulong, ConcurrentTaskQueue> _guildQueues = new();
    private readonly ILogger<TicketQueueService> _logger;
    private readonly ILifetimeScope _lifetimeScope;

    public TicketQueueService(ILogger<TicketQueueService> logger, ILifetimeScope lifetimeScope)
    {
        _logger = logger;
        _lifetimeScope = lifetimeScope;
    }
      
    public async Task EnqueueAsync(TicketOpenReqDto req, DiscordInteraction interaction)
    {
        if (!_guildQueues.TryGetValue(interaction.Guild.Id, out var guildsQueue))
        {
            _logger.LogError($"Couldn't queue ticket creation for {interaction.Guild.Id}, key not found");
            return;
        }

        using var scope = _lifetimeScope.BeginLifetimeScope();
        var handler = scope.Resolve<ICommandHandler<OpenTicketCommand>>();
        await guildsQueue.EnqueueAsync(() => handler.HandleAsync(new OpenTicketCommand(req, interaction)));
    }

    public bool AddGuildQueue(ulong guildId) 
        => this._guildQueues.TryAdd(guildId, new ConcurrentTaskQueue());


    public bool RemoveGuildQueue(ulong guildId)
        => this._guildQueues.TryRemove(guildId, out _);

    public async Task EnqueueAsync(TicketOpenReqDto req)
    {
        if (!_guildQueues.TryGetValue(req.GuildId, out var guildsQueue))
        {
            _logger.LogError($"Couldn't queue ticket creation for {req.GuildId}, key not found");
            return;
        }

        using var scope = _lifetimeScope.BeginLifetimeScope();
        var handler = scope.Resolve<ICommandHandler<OpenTicketCommand>>();
        await guildsQueue.EnqueueAsync(() => handler.HandleAsync(new OpenTicketCommand(req)));
    }
}
