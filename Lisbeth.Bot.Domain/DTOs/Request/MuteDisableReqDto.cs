using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class MuteDisableReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime? LiftedOn { get; set; } = DateTime.Now;
        public ulong LiftedById { get; set; }

        public MuteDisableReqDto()
        {
        }

        public MuteDisableReqDto(ulong user)
        {
            UserId = user;
        }
    }
}
