using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisbeth.Bot.Domain.Entities
{
    public class ModerationConfig
    {
        public long Id { get; set; }
        public ulong? MemberEventsLogChannelId { get; set; }
        public ulong? MessageEventsLogChannelId { get; set; }


        public long GuildId { get; set; }
        public Guild Guild { get; set; }

        [NotMapped]
        public bool ShouldLogMemberEvents => MemberEventsLogChannelId.HasValue;
        [NotMapped]
        public bool ShouldLogMessageEvents => MessageEventsLogChannelId.HasValue;
    }
}
