using System;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class ServerBooster : DiscordAggregateRootEntity
    {
        public DateTime BoostingSince { get; set; } = DateTime.Now;
        public int BoostCount { get; set; } = 1;
    }
}
