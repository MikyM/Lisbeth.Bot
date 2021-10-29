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
    public class RoleMenuConfig : IEntityTypeConfiguration<RoleMenu>
    {
        public void Configure(EntityTypeBuilder<RoleMenu> builder)
        {
            builder.ToTable("role_menu");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.Name).HasColumnName("name").HasColumnType("varchar(100)").HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.CreatorId).HasColumnName("creator_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.LastEditById).HasColumnName("lasted_edit_by_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.MessageId).HasColumnName("message_id").HasColumnType("bigint");
            builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
            builder.Property(x => x.RoleEmojiMapping)
                .HasColumnName("role_emoji_mapping")
                .HasColumnType("text")
                .HasConversion(x => JsonSerializer.Serialize(x, new JsonSerializerOptions {IgnoreNullValues = true}),
                    x => JsonSerializer.Deserialize<List<RoleEmojiMapping>>(x,
                        new JsonSerializerOptions {IgnoreNullValues = true}));
            builder.Property(x => x.EmbedConfigId).HasColumnName("embed_config_id").HasColumnType("bigint");

            builder.HasOne(x => x.EmbedConfig)
                .WithOne(x => x.RoleMenu)
                .HasForeignKey<RoleMenu>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);
        }
    }
}