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
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MikyM.Common.Utilities;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class GuildEventsHandler : IDiscordGuildEventsSubscriber
{
    private readonly IAsyncExecutor _asyncExecutor;

    public GuildEventsHandler(IAsyncExecutor asyncExecutor)
    {
        _asyncExecutor = asyncExecutor;
    }

    public Task DiscordOnGuildCreated(DiscordClient sender, GuildCreateEventArgs args)
    {
        _ = _asyncExecutor.ExecuteAsync<IDiscordGuildService>(async x => await x.HandleGuildCreateAsync(args));
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildUpdated(DiscordClient sender, GuildUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs args)
    {
        _ = _asyncExecutor.ExecuteAsync<IDiscordGuildService>(async x => await x.HandleGuildDeleteAsync(args));
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildUnavailable(DiscordClient sender, GuildDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }

    public async Task DiscordOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        _ = _asyncExecutor.ExecuteAsync<IDiscordGuildService>(async x =>
            await x.PrepareSlashPermissionsAsync(args.Guilds.Values));
        await sender.UpdateStatusAsync(new DiscordActivity("you closely.", ActivityType.Watching));
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