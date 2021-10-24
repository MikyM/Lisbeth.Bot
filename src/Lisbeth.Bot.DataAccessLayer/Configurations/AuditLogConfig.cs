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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.DataAccessLayer.Configurations
{
    public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_log");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.IsDisabled).HasColumnName("is_disabled").HasColumnType("boolean").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp")
                .ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").IsRequired();

            builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("varchar(100)").HasMaxLength(100)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasColumnType("varchar(100)").HasMaxLength(100)
                .IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.TableName).HasColumnName("table_name").HasColumnType("varchar(200)")
                .HasMaxLength(200).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.OldValues).HasColumnName("old_values").HasColumnType("text").ValueGeneratedOnAdd();
            builder.Property(x => x.NewValues).HasColumnName("new_values").HasColumnType("text").IsRequired()
                .ValueGeneratedOnAdd();
            builder.Property(x => x.AffectedColumns).HasColumnName("affected_columns").HasColumnType("varchar(1000)")
                .HasMaxLength(1000).ValueGeneratedOnAdd();
            builder.Property(x => x.PrimaryKey).HasColumnName("primary_key").HasColumnType("varchar(100)")
                .HasMaxLength(100).IsRequired().ValueGeneratedOnAdd();
        }
    }
}