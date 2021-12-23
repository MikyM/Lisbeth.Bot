using DSharpPlus;
using DSharpPlus.EventArgs;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class PaginationEventsHandler : IDiscordMiscEventsSubscriber
{
    public async Task DiscordOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
    {
        if (args.Id.Contains("pagination", StringComparison.InvariantCultureIgnoreCase))
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}