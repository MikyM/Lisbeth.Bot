﻿// This file is part of Lisbeth.Bot project
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

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

public class EmbedConfigConfig : IEntityTypeConfiguration<EmbedConfig>
{
    public void Configure(EntityTypeBuilder<EmbedConfig> builder)
    {
        builder.ToTable("embed_config");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>().IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("varchar(4096)");
        builder.Property(x => x.Fields)
            .HasColumnName("fields")
            .HasColumnType("text")
            .HasConversion(
                x => JsonSerializer.Serialize(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                x => JsonSerializer.Deserialize<List<DiscordField>>(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
        builder.Property(x => x.Author).HasColumnName("author").HasColumnType("varchar(256)").HasMaxLength(200);
        builder.Property(x => x.AuthorUrl)
            .HasColumnName("author_url")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);
        builder.Property(x => x.Footer).HasColumnName("footer").HasColumnType("varchar(2048)").HasMaxLength(200);
        builder.Property(x => x.FooterImageUrl)
            .HasColumnName("footer_image_url")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);
        builder.Property(x => x.CreatorId).HasColumnName("creator_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.LastEditById).HasColumnName("last_edit_by_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.AuthorImageUrl)
            .HasColumnName("author_image_url")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);
        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);
        builder.Property(x => x.HexColor)
            .HasColumnName("hex_color")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasColumnType("varchar(256)").HasMaxLength(256);
        builder.Property(x => x.Timestamp).HasColumnName("Timestamp").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>();
        builder.Property(x => x.Thumbnail)
            .HasColumnName("thumbnail")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);
        builder.Property(x => x.ThumbnailHeight)
            .HasColumnName("thumbnail_height")
            .HasColumnType("integer");
        builder.Property(x => x.ThumbnailWidth)
            .HasColumnName("thumbnail_width")
            .HasColumnType("integer");
    }
}
