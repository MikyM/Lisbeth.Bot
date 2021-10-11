// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    internal class GuildConfig : IEntityTypeConfiguration<Guild>
    {
        public void Configure(EntityTypeBuilder<Guild> builder)
        {
/*            builder
                .HasOne<TicketingConfig>(x => x.TicketingConfig)
                .WithOne(x => x.Guild)
                .HasForeignKey<TicketingConfig>(x => x.GuildId);

            builder
                .HasOne<ModerationConfig>(x => x.ModerationConfig)
                .WithOne(x => x.Guild)
                .HasForeignKey<ModerationConfig>(x => x.GuildId);*/

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

            builder.Metadata.FindNavigation(nameof(Guild.Bans)).SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Guild.Mutes)).SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Guild.Prunes)).SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.ToTable("guild");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UserId).HasColumnName("inviter_id").HasColumnType("bigint");

            builder.OwnsOne<TicketingConfig>(nameof(Guild.TicketingConfig), options =>
            {
                options.ToTable("guild_ticketing_config");

                options.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
                options.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

                options.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.ClosedCategoryId).HasColumnName("closed_category_id").HasColumnType("bigint");
                options.Property(x => x.OpenedCategoryId).HasColumnName("opened_category_id").HasColumnType("bigint");
                options.Property(x => x.LogChannelId).HasColumnName("log_channel_id").HasColumnType("bigint");
                options.Property(x => x.LastTicketId).HasColumnName("last_ticket_id").HasColumnType("bigint");
                options.Property(x => x.OpenedNamePrefix).HasColumnName("opened_name_prefix").HasColumnType("varchar(100)").HasMaxLength(100);
                //options.Property(x => x.AdditionalInformationCenterMessage).HasColumnName("member_welcome_message").HasColumnType("varchar(5096)").HasMaxLength(5096); to do
                options.Property(x => x.CleanAfter).HasColumnName("clean_after").HasColumnType("time");
                options.Property(x => x.CloseAfter).HasColumnName("close_after").HasColumnType("time");
            });

            builder.OwnsOne<ModerationConfig>(nameof(Guild.ModerationConfig), options =>
            {
                options.ToTable("guild_moderation_config");

                options.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
                options.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

                options.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
                options.Property(x => x.MemberEventsLogChannelId).HasColumnName("member_events_log_channel_id").HasColumnType("bigint");
                options.Property(x => x.MessageDeletedEventsLogChannelId).HasColumnName("message_deleted_events_log_channel_id").HasColumnType("bigint");
                options.Property(x => x.MessageUpdatedEventsLogChannelId).HasColumnName("message_updated_events_log_channel_id").HasColumnType("bigint");
                options.Property(x => x.MuteRoleId).HasColumnName("mute_role_id").HasColumnType("bigint");
                options.Property(x => x.MemberWelcomeMessage).HasColumnName("member_welcome_message").HasColumnType("varchar(5096)").HasMaxLength(5096);
                options.Property(x => x.MemberWelcomeMessageTitle).HasColumnName("member_welcome_message_title").HasColumnType("varchar(256)").HasMaxLength(256);
            });
        }
    }
}
