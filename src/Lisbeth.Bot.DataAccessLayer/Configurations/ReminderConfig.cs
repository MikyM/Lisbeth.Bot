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
            builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text").IsRequired();
            builder.Property(x => x.SetForDate).HasColumnName("set_for_date").HasColumnType("timestamptz").IsRequired();
        }
    }
}
