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
using System.Text.Json.Serialization;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class RecurringReminderConfig : IEntityTypeConfiguration<RecurringReminder>
    {
        public void Configure(EntityTypeBuilder<RecurringReminder> builder)
        {
            builder.ToTable("recurring_reminder");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();

            builder.Property(x => x.Mentions)
                .HasColumnName("tags")
                .HasColumnType("text")
                .HasConversion(
                    x => JsonSerializer.Serialize(x,
                        new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                    x => JsonSerializer.Deserialize<List<string>>(x,
                        new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired()
                .ValueGeneratedOnAdd();
            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint");
            builder.Property(x => x.CreatorId).HasColumnName("creator_id").HasColumnType("bigint").IsRequired();
            builder.Property(x => x.LastEditById)
                .HasColumnName("lasted_edit_by_id")
                .HasColumnType("bigint")
                .IsRequired();
            builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
            builder.Property(x => x.IsGuildReminder)
                .HasColumnName("is_guild_reminder")
                .HasColumnType("boolean")
                .IsRequired();
            builder.Property(x => x.CronExpression)
                .HasColumnName("cron_expression")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.EmbedConfigId).HasColumnName("embed_config_id").HasColumnType("bigint");
            builder.Property(x => x.HangfireId).HasColumnName("hangfire_id").HasColumnType("varchar(300)").HasMaxLength(300);
            builder.Property(x => x.ChannelId).HasColumnName("bigint");

            builder.HasOne(x => x.EmbedConfig)
                .WithOne(x => x.RecurringReminder)
                .HasForeignKey<RecurringReminder>(x => x.EmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);
        }
    }
}