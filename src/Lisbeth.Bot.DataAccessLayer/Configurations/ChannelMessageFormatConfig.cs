// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

public class ChannelMessageFormatConfig : IEntityTypeConfiguration<ChannelMessageFormat>
{
    public void Configure(EntityTypeBuilder<ChannelMessageFormat> builder)
    {
        builder.ToTable("channel_message_format");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();

        builder.Property(x => x.GuildId)
            .HasColumnName("guild_id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.MessageFormat)
            .HasColumnName("message_format")
            .HasColumnType("varchar(2000)")
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(x => x.CreatorId)
            .HasColumnName("creator_id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.LastEditById)
            .HasColumnName("last_edit_by_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasIndex(x => x.ChannelId);
    }
}