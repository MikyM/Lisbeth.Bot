using DSharpPlus.EventArgs;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

public class ZombiedEventHandler : IDiscordWebSocketEventsSubscriber
{
    public Task DiscordOnSocketErrored(DiscordClient sender, SocketErrorEventArgs args)
        => Task.CompletedTask;
    

    public Task DiscordOnSocketOpened(DiscordClient sender, SocketEventArgs args)
        => Task.CompletedTask;

    public Task DiscordOnSocketClosed(DiscordClient sender, SocketCloseEventArgs args)
        => Task.CompletedTask;

    public Task DiscordOnReady(DiscordClient sender, SessionReadyEventArgs args)
        => Task.CompletedTask;

    public Task DiscordOnResumed(DiscordClient sender, SessionReadyEventArgs args)
        => Task.CompletedTask;

    public Task DiscordOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs args)
        => Task.CompletedTask;

    public Task DiscordOnHeartbeated(DiscordClient sender, HeartbeatEventArgs args)
        => Task.CompletedTask;

    public async Task DiscordOnZombied(DiscordClient sender, ZombiedEventArgs args)
    {
        await sender.ReconnectAsync(true);
    }
}
