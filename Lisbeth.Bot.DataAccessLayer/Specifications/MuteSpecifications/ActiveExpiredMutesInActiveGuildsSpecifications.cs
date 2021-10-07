using System;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications
{
    public class ActiveExpiredMutesInActiveGuildsSpecifications : Specifications<Mute>
    {
        public ActiveExpiredMutesInActiveGuildsSpecifications()
        {
            AddFilterCondition(x => !x.IsDisabled);
            AddFilterCondition(x => !x.Guild.IsDisabled);
            AddFilterCondition(x => x.AppliedUntil <= DateTime.UtcNow);
            AddInclude(x => x.Guild);
            ApplyOrderBy(x => x.Guild.Id);
        }
    }
}
