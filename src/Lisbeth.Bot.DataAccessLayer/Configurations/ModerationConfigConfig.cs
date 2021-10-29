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


using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class ModerationConfigConfig : IEntityTypeConfiguration<ModerationConfig>
    {
        public void Configure(EntityTypeBuilder<ModerationConfig> builder)
        {
            builder.ToTable("moderation_config");

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
            builder.Property(x => x.MemberEventsLogChannelId)
                .HasColumnName("member_events_log_channel_id").HasColumnType("bigint");
            builder.Property(x => x.MessageDeletedEventsLogChannelId)
                .HasColumnName("message_deleted_events_log_channel_id").HasColumnType("bigint");
            builder.Property(x => x.MessageUpdatedEventsLogChannelId)
                .HasColumnName("message_updated_events_log_channel_id").HasColumnType("bigint");
            builder.Property(x => x.MuteRoleId).HasColumnName("mute_role_id")
                .HasColumnType("bigint");
            builder.Property(x => x.BaseMemberWelcomeMessage).HasColumnName("member_welcome_message")
                .HasColumnType("text");
            builder.Property(x => x.MemberWelcomeEmbedConfigId)
                .HasColumnName("member_welcome_embed_config_id")
                .HasColumnType("bigint");

            builder.HasOne(x => x.MemberWelcomeEmbedConfig)
                .WithOne(x => x.ModerationConfig)
                .HasForeignKey<ModerationConfig>(x => x.MemberWelcomeEmbedConfigId)
                .HasPrincipalKey<EmbedConfig>(x => x.Id)
                .IsRequired(false);
        }
    }
}
