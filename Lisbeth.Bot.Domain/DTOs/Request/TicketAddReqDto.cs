namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class TicketAddReqDto
    {
        public long? Id { get; set; }
        public ulong? OwnerId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? GuildSpecificId { get; set; }
        public ulong RequestedById { get; set; }
        public ulong SnowflakeId { get; set; }

        public TicketAddReqDto(long? id, ulong? ownerId, ulong? guildId, ulong? channelId, ulong requestedById, ulong snowflakeId)
        {
            Id = id;
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = channelId;
            RequestedById = requestedById;
            SnowflakeId = snowflakeId;
        }
    }
}
