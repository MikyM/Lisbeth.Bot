namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class TicketExportReqDto
    {
        public long? TicketId { get; set; }
        public ulong GuildId { get; set; }
        public ulong OwnerId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
