using MikyM.Common.EfCore.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;

public class ActiveTicketByUserIdSpec : Specification<Domain.Entities.Ticket>
{
    public ActiveTicketByUserIdSpec(ulong userId)
    {
        Where(x => !x.IsDisabled);
        Where(x => x.UserId == userId);
    }
}
