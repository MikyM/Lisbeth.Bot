using MikyM.Common.EfCore.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;

public class TicketByChannelIdOrGuildAndOwnerIdSpec : Specification<Domain.Entities.Ticket>
{
    public TicketByChannelIdOrGuildAndOwnerIdSpec(ulong? channelId, ulong? guildId, ulong? ownerId)
    {
        if (channelId is not null) Where(x => x.ChannelId == channelId);
        if (guildId is not null) Where(x => x.GuildId == guildId);
        if (ownerId is not null) Where(x => x.UserId == ownerId);
    }
}
