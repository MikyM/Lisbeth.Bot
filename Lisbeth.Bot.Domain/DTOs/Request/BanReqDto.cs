using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class BanReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime? AppliedUntil { get; set; }
        public ulong AppliedById { get; set; }
        public string Reason { get; set; }

        public BanReqDto()
        {
        }
        public BanReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil) : this(user, guild, appliedById, appliedUntil, null)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
        }
        public BanReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil, string reason)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
            Reason = reason;
        }
    }
}
