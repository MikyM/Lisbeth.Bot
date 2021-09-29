using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class MuteReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime? MutedUntil { get; set; }
        public ulong MutedById { get; set; }
        public string Reason { get; set; }

        public MuteReqDto()
        {
        }
        public MuteReqDto(ulong user, ulong mutedById, DateTime? mutedUntil) : this(user, mutedById, mutedUntil, null)
        {
            UserId = user;
            MutedUntil = mutedUntil;
            MutedById = mutedById;
        }
        public MuteReqDto(ulong user, ulong mutedById, DateTime? mutedUntil, string reason)
        {
            UserId = user;
            MutedUntil = mutedUntil;
            MutedById = mutedById;
            Reason = reason;
        }
    }
}
