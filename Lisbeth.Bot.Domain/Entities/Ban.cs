using Lisbeth.Bot.Domain.Entities.Base;
using System;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Ban : DiscordAggregateRootEntity
    {
        public DateTime? LiftedOn { get; set; }
        public DateTime? BannedUntil { get; set; }
        public DateTime? BannedOn { get; set; } = DateTime.UtcNow;
        public ulong BannedById { get; set; }
        public ulong LiftedById { get; set; }
        public string Reason { get; set; } = "";

        public Ban ShallowCopy()
        {
            return (Ban)this.MemberwiseClone();
        }
    }
}
