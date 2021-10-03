using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications
{
    public class TicketBaseGetSpecifications : Specifications<Ticket>
    {
        public TicketBaseGetSpecifications(long? id = null, ulong? userId = null, ulong? guildId = null, ulong? channelId = null, int limit = 0)
        {
            if (id is not null)
                ApplyFilterCondition(x => x.Id == id);
            if (userId is not null)
                ApplyFilterCondition(x => x.UserId == userId);
            if (guildId is not null)
                ApplyFilterCondition(x => x.GuildId == guildId);
            if (channelId is not null)
                ApplyFilterCondition((x => x.ChannelId == channelId));

            ApplyOrderByDescending(x => x.CreatedAt);

            ApplyLimit(limit);
        }
    }
}
