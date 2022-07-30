// This file is part of Lisbeth.Bot project
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

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

public class MemberHistoryEntryConfig : IEntityTypeConfiguration<MemberHistoryEntry>
{
    public void Configure(EntityTypeBuilder<MemberHistoryEntry> builder)
    {
        builder.ToTable("member_history_entry");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz")
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();

        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.Username).HasColumnName("username").HasColumnType("text").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.AccountCreated).HasColumnName("account_created").HasColumnType("timestamptz").ValueGeneratedOnAdd()
            .IsRequired();
        
        builder
            .HasMany(x => x.ServerBoosterHistoryEntries)
            .WithOne(x => x.MemberHistoryEntry)
            .HasForeignKey(x => new {x.GuildId, x.UserId} )
            .HasPrincipalKey(x => new {x.GuildId, x.UserId} )
            .IsRequired(false);
    }
}
