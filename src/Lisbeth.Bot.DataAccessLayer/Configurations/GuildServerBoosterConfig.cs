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

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

public class GuildServerBoosterConfig : IEntityTypeConfiguration<GuildServerBooster>
{
    public void Configure(EntityTypeBuilder<GuildServerBooster> builder)
    {
        builder.ToTable("guild_server_booster");

        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.ServerBoosterId).HasColumnName("user_id").HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasKey(x => new { x.GuildId, x.ServerBoosterId });

        builder
            .HasOne(x => x.ServerBooster)
            .WithMany(x => x.GuildServerBoosters)
            .HasForeignKey(x => x.ServerBoosterId)
            .HasPrincipalKey(x => x.UserId);


        builder
            .HasOne(x => x.Guild)
            .WithMany(x => x.GuildServerBoosters)
            .HasForeignKey(x => x.GuildId)
            .HasPrincipalKey(x => x.GuildId);
    }
}