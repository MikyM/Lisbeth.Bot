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

public class TicketConfig : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("ticket");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>().IsRequired();

        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.ChannelId).HasColumnName("channel_id").HasColumnType("bigint");
        builder.Property(x => x.GuildSpecificId).HasColumnName("guild_specific_id").HasColumnType("bigint");
        builder.Property(x => x.MessageOpenId).HasColumnName("message_open_id").HasColumnType("bigint");
        builder.Property(x => x.MessageCloseId).HasColumnName("message_close_id").HasColumnType("bigint");
        builder.Property(x => x.MessageReopenId).HasColumnName("message_reopen_id").HasColumnType("bigint");
        builder.Property(x => x.ClosedById).HasColumnName("closed_by_id").HasColumnType("bigint");
        builder.Property(x => x.ClosedOn).HasColumnName("closed_on").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>();
        builder.Property(x => x.ReopenedById).HasColumnName("reopened_by_id").HasColumnType("bigint");
        builder.Property(x => x.ReopenedOn).HasColumnName("reopened_on").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>();
        builder.Property(x => x.AddedRoleIds)
            .HasColumnName("added_roles")
            .HasColumnType("text")
            .HasConversion(
                x => JsonSerializer.Serialize(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                x => JsonSerializer.Deserialize<List<ulong>>(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
        builder.Property(x => x.AddedUserIds)
            .HasColumnName("added_users")
            .HasColumnType("text")
            .HasConversion(
                x => JsonSerializer.Serialize(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                x => JsonSerializer.Deserialize<List<ulong>>(x,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
        builder.Property(x => x.IsPrivate).HasColumnName("is_private").HasColumnType("boolean").IsRequired();
    }
}
