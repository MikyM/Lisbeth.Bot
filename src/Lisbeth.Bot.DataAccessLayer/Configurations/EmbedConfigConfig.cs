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

using System.Collections.Generic;
using System.Text.Json;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class EmbedConfigConfig : IEntityTypeConfiguration<EmbedConfig>
    {
        public void Configure(EntityTypeBuilder<EmbedConfig> builder)
        {
            builder.ToTable("embed_config");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean")
                .IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp")
                .IsRequired();

            builder.Property(x => x.Description).HasColumnName("description").HasColumnType("varchar(4096)");
            builder.Property(x => x.Fields)
                .HasColumnName("fields")
                .HasColumnType("text")
                .HasConversion(x => JsonSerializer.Serialize(x, new JsonSerializerOptions {IgnoreNullValues = true}),
                    x => JsonSerializer.Deserialize<List<DiscordField>>(x,
                        new JsonSerializerOptions {IgnoreNullValues = true}));
            builder.Property(x => x.Author).HasColumnName("author").HasColumnType("varchar(256)")
                .HasMaxLength(200);
            builder.Property(x => x.AuthorUrl).HasColumnName("author_url").HasColumnType("varchar(1000)")
                .HasMaxLength(200);
            builder.Property(x => x.Footer).HasColumnName("footer").HasColumnType("varchar(2048)")
                .HasMaxLength(200);
            builder.Property(x => x.FooterImageUrl).HasColumnName("footer_image_url")
                .HasColumnType("varchar(1000)").HasMaxLength(1000);
            builder.Property(x => x.AuthorImageUrl).HasColumnName("author_image_url")
                .HasColumnType("varchar(1000)").HasMaxLength(1000);
            builder.Property(x => x.ImageUrl).HasColumnName("image_url")
                .HasColumnType("varchar(1000)").HasMaxLength(1000);
            builder.Property(x => x.HexColor).HasColumnName("hex_color").HasColumnType("varchar(40)")
                .HasMaxLength(40).IsRequired();
            builder.Property(x => x.Title).HasColumnName("title").HasColumnType("varchar(256)")
                .HasMaxLength(256);
            builder.Property(x => x.Timestamp).HasColumnName("timestamp").HasColumnType("timestamp");
            builder.Property(x => x.Thumbnail).HasColumnName("thumbnail").HasColumnType("varchar(100)")
                .HasMaxLength(100);
            builder.Property(x => x.ThumbnailHeight).HasColumnName("thumbnail_height").HasColumnType("integer")
                .IsRequired();
            builder.Property(x => x.ThumbnailWidth).HasColumnName("thumbnail_width").HasColumnType("integer")
                .IsRequired();

            builder.HasOne(x => x.Reminder)
                .WithOne(x => x.EmbedConfig)
                .HasForeignKey<Reminder>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);

            builder.HasOne(x => x.RecurringReminder)
                .WithOne(x => x.EmbedConfig)
                .HasForeignKey<RecurringReminder>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);

            builder.HasOne(x => x.Tag)
                .WithOne(x => x.EmbedConfig)
                .HasForeignKey<Tag>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);

            builder.HasOne(x => x.RoleMenu)
                .WithOne(x => x.EmbedConfig)
                .HasForeignKey<RoleMenu>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);
        }
    }
}