using DSharpPlus;
using DSharpPlus.EventArgs;
using MikyM.Discord.Events;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class TicketEvents : IDiscordMiscEventsSubscriber
    {
        private readonly IDiscordTicketService _discordTicketService;

        public TicketEvents(IDiscordTicketService discordTicketService)
        {
            _discordTicketService = discordTicketService;
        }

        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (!args.Channel.Name.StartsWith("ticket-"))
                return;
            await args.Interaction.CreateResponseAsync(InteractionResponseType.Pong);
            if (args.Id == "close-ticket-btn")
            {
                _ = Task.Run(async () =>
                {
                    var req = new TicketCloseReqDto(null, null, args.Interaction.GuildId, args.Interaction.ChannelId, args.Interaction.User.Id);
                    return _discordTicketService.CloseTicketAsync(args.Interaction);
                });
            }

            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }


        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
