using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class RecurringReminder : SnowflakeEntity
    {
        public string CronExpression { get; set; }
        public ulong? UserId { get; set; }
        public ulong? GuildId { get; set; }
        public string Text { get; set; }
        public long ReminderEmbedConfigId { get; set; }

        public ReminderEmbedConfig ReminderEmbedConfig { get; set; }
        public Guild Guild { get; set; }
    }
}
