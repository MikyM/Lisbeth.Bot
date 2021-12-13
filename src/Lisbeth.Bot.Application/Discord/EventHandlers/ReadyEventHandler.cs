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
using DSharpPlus.EventArgs;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

public class ReadyEventHandler : IDiscordWebSocketEventsSubscriber
{
    public ReadyEventHandler()
    {
    }

    public Task DiscordOnSocketErrored(DiscordClient sender, SocketErrorEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnSocketOpened(DiscordClient sender, SocketEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnSocketClosed(DiscordClient sender, SocketCloseEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnReady(DiscordClient sender, ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnResumed(DiscordClient sender, ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnHeartbeated(DiscordClient sender, HeartbeatEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnZombied(DiscordClient sender, ZombiedEventArgs args)
    {
        return Task.CompletedTask;
    }
}