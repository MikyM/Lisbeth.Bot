using DSharpPlus;
using DSharpPlus.EventArgs;
using MikyM.Discord.Events;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class TicketEvents : IDiscordMiscEventsSubscriber
    {
        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (!args.Channel.Name.StartsWith("ticket-"))
                return;

            if (args.Id == "close-ticket-btn")
                _ = Task.Run(async () => CloseTicketAsync(args.Interaction));

            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }


        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
