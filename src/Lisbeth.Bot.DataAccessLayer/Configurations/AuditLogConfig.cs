using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_log");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("varchar(100)").HasMaxLength(100).ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.TableName).HasColumnName("table_name").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.OldValues).HasColumnName("old_values").HasColumnType("text").ValueGeneratedOnAdd();
            builder.Property(x => x.NewValues).HasColumnName("new_values").HasColumnType("text").IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.AffectedColumns).HasColumnName("affected_columns").HasColumnType("varchar(1000)").HasMaxLength(1000).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.PrimaryKey).HasColumnName("primary_key").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired().ValueGeneratedOnAdd();
        }
    }
}