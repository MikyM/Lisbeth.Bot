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
using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    internal class GuildConfig : IEntityTypeConfiguration<Guild>
    {
        public void Configure(EntityTypeBuilder<Guild> builder)
        {
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

            builder.Metadata.FindNavigation(nameof(Guild.Bans)).SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Guild.Mutes)).SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Guild.Prunes)).SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Guild.GuildServerBoosters)).SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.ToTable("guild");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.UserId).HasColumnName("inviter_id").HasColumnType("bigint");
            builder.Property(x => x.EmbedHexColor).HasColumnName("embed_hex_color").HasColumnType("varchar(40)")
                .HasMaxLength(40).IsRequired();

            builder.OwnsOne<TicketingConfig>(nameof(Guild.TicketingConfig), ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("guild_ticketing_config");

                ownedNavigationBuilder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean")
                    .IsRequired();
                ownedNavigationBuilder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp")
                    .IsRequired();

                ownedNavigationBuilder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.ClosedCategoryId).HasColumnName("closed_category_id")
                    .HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.OpenedCategoryId).HasColumnName("opened_category_id")
                    .HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.LogChannelId).HasColumnName("log_channel_id")
                    .HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.LastTicketId).HasColumnName("last_ticket_id")
                    .HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.OpenedNamePrefix).HasColumnName("opened_name_prefix")
                    .HasColumnType("varchar(100)").HasMaxLength(100);
                ownedNavigationBuilder.Property(x => x.ClosedNamePrefix).HasColumnName("closed_name_prefix")
                    .HasColumnType("varchar(100)").HasMaxLength(100);
                ownedNavigationBuilder.Property(x => x.CleanAfter).HasColumnName("clean_after").HasColumnType("time");
                ownedNavigationBuilder.Property(x => x.CloseAfter).HasColumnName("close_after").HasColumnType("time");
                ownedNavigationBuilder.Property(x => x.TicketCenterMessageDescription).HasColumnName("ticket_center_message_description")
                    .HasColumnType("text");
                ownedNavigationBuilder.Property(x => x.TicketWelcomeMessageDescription).HasColumnName("ticket_welcome_message_description")
                    .HasColumnType("text");
                ownedNavigationBuilder.Property(x => x.TicketCenterMessageFields).HasColumnName("ticket_center_message_fields")
                    .HasColumnType("text");
                ownedNavigationBuilder.Property(x => x.TicketWelcomeMessageFields).HasColumnName("ticket_welcome_message_fields")
                    .HasColumnType("text");

                ownedNavigationBuilder
                    .WithOwner(x => x.Guild)
                    .HasForeignKey(x => x.GuildId)
                    .HasPrincipalKey(x => x.Id);
            });

            builder.OwnsOne<ModerationConfig>(nameof(Guild.ModerationConfig), ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("guild_moderation_config");

                ownedNavigationBuilder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean")
                    .IsRequired();
                ownedNavigationBuilder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp")
                    .IsRequired();

                ownedNavigationBuilder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint")
                    .ValueGeneratedOnAdd().IsRequired();
                ownedNavigationBuilder.Property(x => x.MemberEventsLogChannelId)
                    .HasColumnName("member_events_log_channel_id").HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.MessageDeletedEventsLogChannelId)
                    .HasColumnName("message_deleted_events_log_channel_id").HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.MessageUpdatedEventsLogChannelId)
                    .HasColumnName("message_updated_events_log_channel_id").HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.MuteRoleId).HasColumnName("mute_role_id")
                    .HasColumnType("bigint");
                ownedNavigationBuilder.Property(x => x.MemberWelcomeMessage).HasColumnName("member_welcome_message")
                    .HasColumnType("text");
                ownedNavigationBuilder.Property(x => x.MemberWelcomeMessageTitle)
                    .HasColumnName("member_welcome_message_title").HasColumnType("varchar(256)").HasMaxLength(256);

                ownedNavigationBuilder
                    .WithOwner(x => x.Guild)
                    .HasForeignKey(x => x.GuildId)
                    .HasPrincipalKey(x => x.Id);
            });
        }
    }
}