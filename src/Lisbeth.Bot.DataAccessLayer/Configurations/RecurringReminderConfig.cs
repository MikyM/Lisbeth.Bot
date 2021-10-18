using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class RecurringReminderConfig : IEntityTypeConfiguration<RecurringReminder>
    {
        public void Configure(EntityTypeBuilder<RecurringReminder> builder)
        {
            builder.ToTable("recurring_reminder");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint");
            builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
            builder.Property(x => x.CronExpression).HasColumnName("cron_expression").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();

            builder.OwnsOne<ReminderEmbedConfig>(nameof(Reminder.ReminderEmbedConfig), ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("recurring_reminder_embed_config");

                ownedNavigationBuilder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
                ownedNavigationBuilder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

                ownedNavigationBuilder.Property(x => x.Description).HasColumnName("description").HasColumnName("text");
                ownedNavigationBuilder.Property(x => x.Fields).HasColumnName("fields").HasColumnName("text");
                ownedNavigationBuilder.Property(x => x.Title).HasColumnName("title").HasColumnName("varchar(200)").HasMaxLength(200);
                ownedNavigationBuilder.Property(x => x.Footer).HasColumnName("footer").HasColumnName("varchar(200)").HasMaxLength(200);
                ownedNavigationBuilder.Property(x => x.FooterImageUrl).HasColumnName("footer_image_url").HasColumnName("varchar(1000)").HasMaxLength(1000);
                ownedNavigationBuilder.Property(x => x.TitleImageUrl).HasColumnName("title_image_url").HasColumnName("varchar(1000)").HasMaxLength(1000);
                ownedNavigationBuilder.Property(x => x.ImageUrl).HasColumnName("image_url").HasColumnName("varchar(1000)").HasMaxLength(1000);

                ownedNavigationBuilder
                    .WithOwner(x => x.RecurringReminder)
                    .HasForeignKey(x => x.RecurringReminderId)
                    .HasPrincipalKey(x => x.Id);
            });
        }
    }
}
