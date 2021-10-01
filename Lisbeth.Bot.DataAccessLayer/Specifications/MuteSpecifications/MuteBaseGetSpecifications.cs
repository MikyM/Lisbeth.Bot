using System;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications
{
    public class MuteBaseGetSpecifications : Specifications<Mute>
    {
        public MuteBaseGetSpecifications(long? id = null, ulong? userId = null, ulong? guildId = null, ulong? appliedById = null, DateTime? liftedOn = null, DateTime? appliedOn = null, ulong? liftedById = null, int limit = 0)
        {
            if(id != null)
                ApplyFilterCondition(x => x.Id == id);
            if (userId != null)
                ApplyFilterCondition(x => x.UserId == userId);
            if (guildId != null)
                ApplyFilterCondition(x => x.GuildId == guildId);
            if (appliedById != null)
                ApplyFilterCondition(x => x.AppliedById == appliedById);
            if (liftedById != null)
                ApplyFilterCondition(x => x.LiftedById == liftedById);
            if (liftedOn != null)
                ApplyFilterCondition(x => x.LiftedOn == liftedOn);
            if (appliedOn != null)
                ApplyFilterCondition(x => x.AppliedOn == appliedOn);
            if (liftedById != null)
                ApplyFilterCondition(x => x.LiftedById == liftedById);

            ApplyOrderByDescending(x => x.AppliedOn);

            ApplyLimit(limit);
        }
    }
}
