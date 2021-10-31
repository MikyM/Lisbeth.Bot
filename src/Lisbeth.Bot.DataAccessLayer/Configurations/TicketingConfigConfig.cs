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


using System;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class TicketingConfigConfig : IEntityTypeConfiguration<TicketingConfig>
    {
        public void Configure(EntityTypeBuilder<TicketingConfig> builder)
        {
            builder.ToTable("ticketing_config");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean")
                .IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp")
                .IsRequired();

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.ClosedCategoryId).HasColumnName("closed_category_id")
                .HasColumnType("bigint").IsRequired();
            builder.Property(x => x.OpenedCategoryId).HasColumnName("opened_category_id")
                .HasColumnType("bigint").IsRequired();
            builder.Property(x => x.LogChannelId).HasColumnName("log_channel_id")
                .HasColumnType("bigint").IsRequired();
            builder.Property(x => x.LastTicketId).HasColumnName("last_ticket_id")
                .HasColumnType("bigint");
            builder.Property(x => x.OpenedNamePrefix).HasColumnName("opened_name_prefix")
                .HasColumnType("varchar(100)").HasMaxLength(100);
            builder.Property(x => x.ClosedNamePrefix)
                .HasColumnName("closed_name_prefix")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);
            builder.Property(x => x.CleanAfter)
                .HasColumnName("clean_after")
                .HasColumnType("bigint")
                .HasConversion(x => x.Value.Ticks, x => TimeSpan.FromTicks(x));
            builder.Property(x => x.CloseAfter)
                .HasColumnName("close_after")
                .HasColumnType("bigint")
                .HasConversion(x => x.Value.Ticks, x => TimeSpan.FromTicks(x));
            builder.Property(x => x.BaseCenterMessage)
                .HasColumnName("center_message_description")
                .HasColumnType("text");
            builder.Property(x => x.BaseWelcomeMessage)
                .HasColumnName("welcome_message_description")
                .HasColumnType("text");
            builder.Property(x => x.CenterEmbedConfigId)
                .HasColumnName("center_embed_config_id")
                .HasColumnType("bigint");
            builder.Property(x => x.WelcomeEmbedConfigId)
                .HasColumnName("welcome_embed_config_id")
                .HasColumnType("bigint");

            builder.HasOne(x => x.WelcomeEmbedConfig)
                .WithOne(x => x.TicketingConfigWithWelcomeMessage)
                .HasForeignKey<TicketingConfig>(x => x.WelcomeEmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);

            builder.HasOne(x => x.CenterEmbedConfig)
                .WithOne(x => x.TicketingConfigWithCenterMessage)
                .HasForeignKey<TicketingConfig>(x => x.CenterEmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);
        }
    }
}