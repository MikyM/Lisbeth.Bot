namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class PruneReqDto
    {
        public int Count { get; set; } = 100;
        public ulong? UserId { get; set; }
        public ulong? MessageId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? RequestedById { get; set; }

        public PruneReqDto(int count, ulong? messageId = null, ulong? userId = null, ulong? channelId = null, ulong? guildId = null, ulong? requestedById = null)
        {
            Count = count;
            MessageId = messageId;
            UserId = userId;
            ChannelId = channelId;
            GuildId = guildId;
            RequestedById = requestedById;
        }
    }
}
