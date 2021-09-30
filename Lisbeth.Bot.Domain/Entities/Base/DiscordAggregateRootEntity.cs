using System.ComponentModel.DataAnnotations.Schema;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities.Base
{
    public class DiscordAggregateRootEntity : AggregateRootEntity
    {
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
    }
}
