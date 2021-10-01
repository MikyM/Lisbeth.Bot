using System;

namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class MuteGetReqDto
    {
        public long? Id { get; set; }
        public ulong? UserId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? AppliedById { get; set; }
        public DateTime? LiftedOn { get; set; }
        public DateTime? AppliedOn { get; set; }
        public ulong? LiftedById { get; set; }

        public MuteGetReqDto(long? id = null, ulong? userId = null, ulong? guildId = null, ulong? appliedById = null, DateTime? liftedOn = null, DateTime? appliedOn = null, ulong? liftedById = null)
        {
            Id = id;
            UserId = userId;
            GuildId = guildId;
            AppliedById = appliedById;
            LiftedOn = liftedOn;
            AppliedOn = appliedOn;
            LiftedById = liftedById;
        }
    }
}
