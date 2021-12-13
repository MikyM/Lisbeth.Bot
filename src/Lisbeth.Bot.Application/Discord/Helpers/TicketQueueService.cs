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
using Serilog;
using System.Collections.Concurrent;
using System.Threading;
using MikyM.Common.Utilities;

namespace Lisbeth.Bot.Application.Discord.Helpers;

public interface ITicketQueueService
{
    Task EnqueueAsync(TicketOpenReqDto req);
    Task EnqueueAsync(TicketOpenReqDto req, DiscordInteraction args);
    bool AddGuildQueue(ulong guildId);
    bool RemoveGuildQueue(ulong guildId);
}

public class TicketQueueService : ITicketQueueService
{
    private readonly ConcurrentDictionary<ulong, TaskConcurrentQueue> _cache = new();
    private readonly ILogger<TicketQueueService> _logger;
    private readonly ILifetimeScope _lifetimeScope;

    public TicketQueueService(ILogger<TicketQueueService> logger, ILifetimeScope lifetimeScope)
    {
        _logger = logger;
        _lifetimeScope = lifetimeScope;
    }

    public async Task EnqueueAsync(TicketOpenReqDto req, DiscordInteraction args)
    {
        if (!_cache.TryGetValue(args.Guild.Id, out var guildsQueue))
        {
            _logger.LogError($"Couldn't queue ticket creation for {args.Guild.Id}, key not found");
            return;
        }

        var service = _lifetimeScope.BeginLifetimeScope().Resolve<IDiscordTicketService>();
        await guildsQueue.EnqueueAsync(() => service.OpenTicketAsync(args, req));
    }

    public bool AddGuildQueue(ulong guildId)
    {
        _logger.LogDebug($"Adding guild with Id: {guildId}");
        return this._cache.TryAdd(guildId, new TaskConcurrentQueue());
    }

    public bool RemoveGuildQueue(ulong guildId)
        => this._cache.TryRemove(guildId, out _);

    public async Task EnqueueAsync(TicketOpenReqDto req)
    {
        if (!_cache.TryGetValue(req.GuildId, out var guildsQueue))
        {
            _logger.LogError($"Couldn't queue ticket creation for {req.GuildId}, key not found");
            return;
        }

        var service = _lifetimeScope.BeginLifetimeScope().Resolve<IDiscordTicketService>();
        await guildsQueue.EnqueueAsync(() => service.OpenTicketAsync(req));
    }
}
