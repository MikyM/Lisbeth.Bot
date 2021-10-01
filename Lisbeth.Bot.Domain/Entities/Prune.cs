using System;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Prune : AggregateRootEntity
    {
        public ulong ModeratorId { get; set; }
        public ulong TargetAuthorId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string Messages { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
