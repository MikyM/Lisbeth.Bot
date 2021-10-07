using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications
{
    public class ActiveGuildByDiscordIdWithIncludeSpecifications : Specifications<Guild>
    {
        public ActiveGuildByDiscordIdWithIncludeSpecifications(ulong discordGuildId)
        {
            ApplyFilterCondition(x => !x.IsDisabled);
            ApplyFilterCondition(x => x.GuildId == discordGuildId);
            AddInclude(x => x.TicketingConfig);
            AddInclude(x => x.ModerationConfig);
        }
    }
}
