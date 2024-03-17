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

public class RoleMenuOptionConfig : IEntityTypeConfiguration<RoleMenuOption>
{
    public void Configure(EntityTypeBuilder<RoleMenuOption> builder)
    {
        builder.ToTable("role_menu_option");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp").HasConversion<DateTimeKindConverter>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasConversion<DateTimeKindConverter>().IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.RoleMenuId).HasColumnName("role_menu_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(256)")
            .HasMaxLength(256);
        builder.Property(x => x.Emoji)
            .HasColumnName("emoji")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);
        builder.Property(x => x.CustomSelectOptionValueId)
            .HasColumnName("custom_select_option_value_id")
            .HasColumnType("varchar(120)")
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("bigint")
            .IsRequired();
    }
}
