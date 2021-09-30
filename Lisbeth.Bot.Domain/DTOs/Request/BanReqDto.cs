using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class BanReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime? BannedUntil { get; set; }
        public ulong BannedById { get; set; }
        public string Reason { get; set; }

        public BanReqDto()
        {
        }
        public BanReqDto(ulong user, ulong guild, ulong bannedById, DateTime? bannedUntil) : this(user, guild, bannedById, bannedUntil, null)
        {
            UserId = user;
            GuildId = guild;
            BannedUntil = bannedUntil;
            BannedById = bannedById;
        }
        public BanReqDto(ulong user, ulong guild, ulong bannedById, DateTime? bannedUntil, string reason)
        {
            UserId = user;
            GuildId = guild;
            BannedUntil = bannedUntil;
            BannedById = bannedById;
            Reason = reason;
        }
    }
}
