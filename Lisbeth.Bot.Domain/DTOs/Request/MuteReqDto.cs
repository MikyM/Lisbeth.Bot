using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class MuteReqDto
    {
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime? AppliedUntil { get; set; }
        public ulong AppliedById { get; set; }
        public string Reason { get; set; }

        public MuteReqDto()
        {
        }
        public MuteReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil) : this(user, guild, appliedById, appliedUntil, null)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
        }
        public MuteReqDto(ulong user, ulong guild, ulong appliedById, DateTime? appliedUntil, string reason)
        {
            UserId = user;
            GuildId = guild;
            AppliedUntil = appliedUntil;
            AppliedById = appliedById;
            Reason = reason;
        }
    }
}
