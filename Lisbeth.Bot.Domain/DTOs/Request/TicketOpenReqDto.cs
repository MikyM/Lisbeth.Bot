namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class TicketOpenReqDto
    {
        public ulong? GuildId { get; set; }
        public ulong? RequestedById { get; set; }
    }
}
