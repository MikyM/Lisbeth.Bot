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

public class RoleMenuConfig : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("role_menu");

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.GuildId).HasColumnName("guild_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.CreatorId).HasColumnName("creator_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.LastEditById)
            .HasColumnName("lasted_edit_by_id")
            .HasColumnType("bigint")
            .IsRequired();
        builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text");
        builder.Property(x => x.EmbedConfigId).HasColumnName("embed_config_id").HasColumnType("bigint");
        builder.Property(x => x.CustomSelectComponentId)
            .HasColumnName("custom_select_component_id")
            .HasColumnType("varchar(120)")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.CustomButtonId)
            .HasColumnName("custom_button_id")
            .HasColumnType("varchar(120)")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasOne(x => x.EmbedConfig)
            .WithOne(x => x.RoleMenu)
            .HasForeignKey<RoleMenu>(x => x.EmbedConfigId)
            .HasPrincipalKey<EmbedConfig>(x => x.Id)
            .IsRequired(false);

        builder.HasMany(x => x.RoleMenuOptions)
            .WithOne(x => x.RoleMenu)
            .HasForeignKey(x => x.RoleMenuId)
            .HasPrincipalKey(x => x.Id);

        builder.Metadata.FindNavigation(nameof(RoleMenu.RoleMenuOptions))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
