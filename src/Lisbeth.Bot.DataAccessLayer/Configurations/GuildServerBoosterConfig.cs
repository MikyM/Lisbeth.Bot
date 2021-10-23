using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class GuildServerBoosterConfig : IEntityTypeConfiguration<GuildServerBooster>
    {
        public void Configure(EntityTypeBuilder<GuildServerBooster> builder)
        {
            builder.ToTable("guild_server_booster");

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.ServerBoosterId).HasColumnName("user_id").HasColumnType("bigint")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasKey(x => new {x.GuildId, x.ServerBoosterId});

            builder
                .HasOne(x => x.ServerBooster)
                .WithMany(x => x.GuildServerBoosters)
                .HasForeignKey(x => x.ServerBoosterId)
                .HasPrincipalKey(x => x.UserId);


            builder
                .HasOne(x => x.Guild)
                .WithMany(x => x.GuildServerBoosters)
                .HasForeignKey(x => x.GuildId)
                .HasPrincipalKey(x => x.GuildId);
        }
    }
}