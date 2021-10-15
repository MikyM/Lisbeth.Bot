using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public sealed class ModerationConfig : SnowflakeEntity
    {
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