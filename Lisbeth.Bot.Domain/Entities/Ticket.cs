using Lisbeth.Bot.Domain.Entities.Base;
using System;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Ticket : DiscordAggregateRootEntity
    {
        public ulong ChannelId { get; set; }
        public DateTime? ReopenedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public ulong? ClosedBy { get; set; }
        public ulong? ReopenedBy { get; set; }
        public ulong MessageOpenId { get; set; }
        public ulong? MessageCloseId { get; set; }
        public ulong? MessageReopenId { get; set; }
        public bool IsPrivate { get; set; } = false;
    }
}
