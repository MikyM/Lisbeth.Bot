using System;
using System.ComponentModel.DataAnnotations.Schema;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Ban : DiscordAggregateRootEntity
    {
        public DateTime LiftedOn { get; set; }
        public DateTime BannedUntil { get; set; }
        public DateTime BannedOn { get; set; } = DateTime.Now;
        public long BannedById { get; set; }
        public long LiftedById { get; set; }
        public string Reason { get; set; } = "";

        [NotMapped]
        public ulong _MutedById
        {
            get
            {
                unchecked
                {
                    return (ulong)BannedById;
                }
            }
            set
            {
                unchecked
                {
                    BannedById = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _LiftedById
        {
            get
            {
                unchecked
                {
                    return (ulong)LiftedById;
                }
            }
            set
            {
                unchecked
                {
                    LiftedById = (long)value;
                }
            }
        }
    }
}
