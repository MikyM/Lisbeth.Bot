using MikyM.Common.EfCore.DataAccessLayer.Specifications;

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
