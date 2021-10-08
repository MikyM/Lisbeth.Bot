using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisbeth.Bot.Domain.Entities
{
    public class ModerationConfig
    {
        public long Id { get; set; }
        public ulong? MemberEventsLogChannelId { get; set; }
        public ulong? MessageDeletedEventsLogChannelId { get; set; }
        public ulong? MessageUpdatedEventsLogChannelId { get; set; }
        public ulong MuteRoleId { get; set; }
        public string MemberWelcomeMessage { get; set; }
        public string MemberWelcomeMessageTitle { get; set; }


        public long GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
