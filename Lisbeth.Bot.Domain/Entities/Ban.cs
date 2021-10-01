using Lisbeth.Bot.Domain.Entities.Base;
using System;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Ban : DiscordAggregateRootEntity
    {
        public DateTime? LiftedOn { get; set; }
        public DateTime? AppliedUntil { get; set; }
        public DateTime? AppliedOn { get; set; } = DateTime.UtcNow;
        public ulong AppliedById { get; set; }
        public ulong LiftedById { get; set; }
        public string Reason { get; set; } = "";

        public Ban ShallowCopy()
        {
            return (Ban)this.MemberwiseClone();
        }
    }
}
