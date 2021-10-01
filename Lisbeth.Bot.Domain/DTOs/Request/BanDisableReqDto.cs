using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class BanDisableReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime? LiftedOn { get; set; } = DateTime.UtcNow;
        public ulong LiftedById { get; set; }

        public BanDisableReqDto()
        {
        }

        public BanDisableReqDto(ulong user, ulong guild, ulong liftedBy)
        {
            UserId = user;
            GuildId = guild;
            LiftedById = liftedBy;
        }
    }
}
