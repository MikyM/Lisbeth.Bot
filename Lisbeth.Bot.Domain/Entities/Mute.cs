using Lisbeth.Bot.Domain.Entities.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Mute : DiscordAggregateRootEntity
    {
        public DateTime? LiftedOn { get; set; }
        public DateTime? MutedUntil { get; set; }
        public DateTime? MutedOn { get; set; } = DateTime.Now;
        public ulong MutedById { get; set; }
        public ulong LiftedById { get; set; }
        public string Reason { get; set; } = "";
        public Mute()
        {
        }

        public Mute(ulong userId, ulong mutedBy, DateTime? mutedUntil) : this(userId, mutedBy, mutedUntil, "")
        {
        }

        public Mute(ulong userId, ulong mutedBy, DateTime? mutedUntil, string reason)
        {
            this.UserId = userId;
            this.MutedById = mutedBy;
            this.MutedUntil = mutedUntil;
            this.Reason = reason;
        }

        public void Lift(ulong liftedBy)
        {
            this.LiftedById = liftedBy;
            this.LiftedOn = DateTime.Now;
        }

        public void Extend(ulong mutedBy, DateTime? mutedUntil, string reason = "No reason provided")
        {
            this.MutedById = mutedBy;
            this.MutedUntil = mutedUntil;
            this.MutedOn = DateTime.Now;
            this.Reason = reason;
        }
    }
}
