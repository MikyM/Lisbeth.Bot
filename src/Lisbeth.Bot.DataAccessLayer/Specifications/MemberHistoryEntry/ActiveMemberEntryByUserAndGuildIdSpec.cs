using DataExplorer.EfCore.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.MemberHistoryEntry;

public class ActiveMemberEntryByUserAndGuildIdSpec : Specification<Domain.Entities.MemberHistoryEntry>
{
    public ActiveMemberEntryByUserAndGuildIdSpec(ulong guildId, ulong userId)
    {
        Where(x => !x.IsDisabled);
        Where(x => x.GuildId == guildId);
        Where(x => x.UserId == userId);
    }
}
