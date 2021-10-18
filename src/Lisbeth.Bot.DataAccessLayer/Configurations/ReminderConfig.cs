using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class ReminderConfig : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.ToTable("reminder");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint");
            builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
            builder.Property(x => x.SetForDate).HasColumnName("set_for_date").HasColumnType("timestamptz").IsRequired();

            builder.OwnsOne<ReminderEmbedConfig>(nameof(Reminder.ReminderEmbedConfig), ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("reminder_embed_config");

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
                    .WithOwner(x => x.Reminder)
                    .HasForeignKey(x => x.ReminderId)
                    .HasPrincipalKey(x => x.Id);
            });
        }
    }
}
