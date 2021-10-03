namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class TicketCloseReqDto
    {
        public long? Id { get; set; }
        public ulong? OwnerId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? RequestedById { get; set; }

        public TicketCloseReqDto(long? id, ulong? ownerId, ulong? guildId, ulong? channelId, ulong? requestedById)
        {
            Id = id;
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = channelId;
            RequestedById = requestedById;
        }
    }
}
