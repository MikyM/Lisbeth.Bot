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

public class TagConfig : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tag");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>().IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasColumnType("varchar(100)").HasMaxLength(100)
            .IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.CreatorId).HasColumnName("creator_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.LastEditById).HasColumnName("lasted_edit_by_id").HasColumnType("bigint")
            .IsRequired();
        builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
        builder.Property(x => x.EmbedConfigId).HasColumnName("embed_config_id").HasColumnType("bigint");
        builder.Property(x => x.AllowedRoleIds)
            .HasColumnName("allowed_role_ids")
            .HasColumnType("text")
            .HasConversion(
                x => JsonSerializer.Serialize(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                x => JsonSerializer.Deserialize<List<ulong>>(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }) ?? new List<ulong>());
        builder.Property(x => x.AllowedUserIds)
            .HasColumnName("allowed_user_ids")
            .HasColumnType("text")
            .HasConversion(
                x => JsonSerializer.Serialize(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                x => JsonSerializer.Deserialize<List<ulong>>(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }) ?? new List<ulong>());

        builder.HasOne(x => x.EmbedConfig)
            .WithOne(x => x.Tag)
            .HasForeignKey<Tag>(x => x.EmbedConfigId)
            .HasPrincipalKey<EmbedConfig>(x => x.Id)
            .IsRequired(false);
    }
}
