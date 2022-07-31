// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Lisbeth.Bot.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

internal class GuildConfig : IEntityTypeConfiguration<Guild>
{
    public void Configure(EntityTypeBuilder<Guild> builder)
    {
        builder.ToTable("guild");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz")
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();

        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UserId).HasColumnName("inviter_id").HasColumnType("bigint");
        builder.Property(x => x.EmbedHexColor).HasColumnName("embed_hex_color").HasColumnType("varchar(40)")
            .HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReminderChannelId).HasColumnName("reminder_channel_id").HasColumnType("bigint");
        builder.Property(x => x.PhishingDetection).HasColumnName("phishing_detection").HasColumnType("varchar(40)").HasMaxLength(40)
            .HasConversion<EnumToStringConverter<PhishingDetection>>().IsRequired();

        builder.Metadata.FindNavigation(nameof(Guild.Tags))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.Bans))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.Mutes))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.Prunes))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.RoleMenus))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.ServerBoosterHistoryEntries))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.MemberHistoryEntries))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.Reminders))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Guild.ChannelMessageFormats))?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.GuildId)
            .IsUnique();

        builder.HasOne(x => x.ModerationConfig)
            .WithOne(x => x.Guild)
            .HasForeignKey<ModerationConfig>(x => x.GuildId)
            .HasPrincipalKey<Guild>(x => x.GuildId);

        builder.HasOne(x => x.TicketingConfig)
            .WithOne(x => x.Guild)
            .HasForeignKey<TicketingConfig>(x => x.GuildId)
            .HasPrincipalKey<Guild>(x => x.GuildId);

        builder
            .HasMany(x => x.Mutes)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.Bans)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.Prunes)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.Tickets)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.RoleMenus)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.ChannelMessageFormats)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.Reminders)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.Tags)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.ServerBoosterHistoryEntries)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);

        builder
            .HasMany(x => x.MemberHistoryEntries)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);
    }
}
