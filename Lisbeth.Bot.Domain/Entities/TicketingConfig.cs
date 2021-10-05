using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisbeth.Bot.Domain.Entities
{
    public class TicketingConfig
    {
        public long Id { get; set; }
        public ulong? LogChannelId { get; set; }
        public long LastTicketId { get; set; }
        public ulong ClosedCategoryId { get; set; }
        public ulong OpenedCategoryId { get; set; }
        public TimeSpan? CleanAfter { get; set; }
        public TimeSpan? CloseAfter { get; set; }
        public string OpenedNamePrefix { get; set; } = "ticket";
        public string ClosedNamePrefix { get; set; } = "closed";
        public string WelcomeMessage { get; set; } = "Welcome @ownerMention@, please be patient, support will be with you shortly!";

        public long GuildId { get; set; }
        public Guild Guild { get; set; }

        [NotMapped]
        public bool ShouldAutoClean => CleanAfter.HasValue;
        [NotMapped]
        public bool ShouldAutoClose => CloseAfter.HasValue;
    }
}
