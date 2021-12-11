using DSharpPlus;
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Helpers;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

public class ReadyEventHandler : IDiscordWebSocketEventsSubscriber
{
    private readonly IAsyncExecutor _asyncExecutor;

    public ReadyEventHandler(IAsyncExecutor asyncExecutor)
    {
        _asyncExecutor = asyncExecutor;
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