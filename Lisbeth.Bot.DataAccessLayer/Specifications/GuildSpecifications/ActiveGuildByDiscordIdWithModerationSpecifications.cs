using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications
{
    public class ActiveGuildByDiscordIdWithModerationSpecifications : Specifications<Guild>
    {
        public ActiveGuildByDiscordIdWithModerationSpecifications(ulong discordGuildId)
        {
            AddFilterCondition(x => !x.IsDisabled);
            AddFilterCondition(x => x.GuildId == discordGuildId);
            AddInclude(x => x.ModerationConfig);
        }
    }
}
