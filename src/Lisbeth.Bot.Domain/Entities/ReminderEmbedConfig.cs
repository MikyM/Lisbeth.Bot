using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class ReminderEmbedConfig : SnowflakeEntity
    {
        public string Fields { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string ImageUrl { get; set; }
        public string FooterImageUrl { get; set; }
        public string TitleImageUrl { get; set; }
        public string Description { get; set; }

        public long? ReminderId { get; set; }
        public long? RecurringReminderId { get; set; }
        public Reminder Reminder { get; set; }
        public RecurringReminder RecurringReminder { get; set; }
    }
}
