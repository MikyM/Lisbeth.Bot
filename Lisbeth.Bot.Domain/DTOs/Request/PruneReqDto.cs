namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class PruneReqDto
    {
        public int Count { get; set; } = 100;
        public ulong? TargetAuthorId { get; set; }
        public ulong? MessageId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ModeratorId { get; set; }

        public PruneReqDto(int count, ulong? messageId = null, ulong? targetAuthorId = null, ulong? channelId = null, ulong? guildId = null, ulong? moderatorId = null)
        {
            Count = count;
            MessageId = messageId;
            TargetAuthorId = targetAuthorId;
            ChannelId = channelId;
            GuildId = guildId;
            ModeratorId = moderatorId;
        }
    }
}
