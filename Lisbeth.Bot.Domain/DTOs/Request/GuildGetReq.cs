namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class GuildGetReq
    {
        public ulong? GuildId { get; set; }
        public ulong? InviterId { get; set; }

        public GuildGetReq(ulong? guildId, ulong? inviterId)
        {
            GuildId = guildId;
            InviterId = inviterId;
        }
    }
}
