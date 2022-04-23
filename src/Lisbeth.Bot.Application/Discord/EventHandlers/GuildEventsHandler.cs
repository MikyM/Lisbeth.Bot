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

using DSharpPlus;
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Helpers;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class GuildEventsHandler : IDiscordGuildEventsSubscriber
{
    private readonly ITicketQueueService _ticketQueueService;
    private readonly IDiscordGuildService _discordGuildService;

    public GuildEventsHandler(ITicketQueueService ticketQueueService,
        IDiscordGuildService discordGuildService)
    {
        _ticketQueueService = ticketQueueService;
        _discordGuildService = discordGuildService;
    }

    public async Task DiscordOnGuildCreated(DiscordClient sender, GuildCreateEventArgs args)
    {
        await _discordGuildService.HandleGuildCreateAsync(args);
        _ticketQueueService.AddGuildQueue(args.Guild.Id);
    }

    public Task DiscordOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildUpdated(DiscordClient sender, GuildUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public async Task DiscordOnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs args)
    {
        await _discordGuildService.HandleGuildDeleteAsync(args);
        _ticketQueueService.RemoveGuildQueue(args.Guild.Id);
    }

    public Task DiscordOnGuildUnavailable(DiscordClient sender, GuildDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }

    public async Task DiscordOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        await _discordGuildService.PrepareSlashPermissionsAsync(args.Guilds.Values);
        await _discordGuildService.PrepareBot(args.Guilds.Keys);
    }

    public Task DiscordOnGuildEmojisUpdated(DiscordClient sender, GuildEmojisUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildStickersUpdated(DiscordClient sender, GuildStickersUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildIntegrationsUpdated(DiscordClient sender, GuildIntegrationsUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }
}