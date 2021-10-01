using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Guild : DiscordAggregateRootEntity
    {
        public ulong InviterId { get; set; }
        public ulong? LogChannelId { get; set; }
        public ulong? TicketLogChannelId { get; set; }
    }
}
