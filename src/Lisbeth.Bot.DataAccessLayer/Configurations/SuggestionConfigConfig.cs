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

public class SuggestionConfigConfig : IEntityTypeConfiguration<Domain.Entities.SuggestionConfig>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SuggestionConfig> builder)
    {
        builder.ToTable("suggestion_config");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint")
            .ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean")
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .IsRequired();

        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint")
            .ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.ShouldCreateThreads)
            .HasColumnName("should_create_threads").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.ShouldAddVoteReactions)
            .HasColumnName("should_add_vote_reactions").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.SuggestionChannelId)
            .HasColumnName("suggestion_channel_id").HasColumnType("bigint").IsRequired();
    }
}
