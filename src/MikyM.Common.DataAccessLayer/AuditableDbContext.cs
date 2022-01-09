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

using MikyM.Common.Domain.Entities;
using System.Collections.Generic;
using System.Threading;

namespace MikyM.Common.DataAccessLayer;

public abstract class AuditableDbContext : DbContext
{
    private string? AuditUserId { get; set; }

    protected AuditableDbContext(DbContextOptions options) : base(options)
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public async Task<int> SaveChangesAsync(string? auditUserId, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.AuditUserId = auditUserId;
        return await this.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(string? auditUserId, CancellationToken cancellationToken = default)
    {
        this.AuditUserId = auditUserId;
        return await this.SaveChangesAsync(true, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges(this.AuditUserId);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void OnBeforeSaveChanges(string? userId)
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry) { TableName = entry.Entity.GetType().Name, UserId = userId };

            auditEntries.Add(auditEntry);

            if (entry.Entity is Entity entity)
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entry.Property("CreatedAt").IsModified = true;
                        break;
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        entry.Property("UpdatedAt").IsModified = true;
                        entry.Property("CreatedAt").IsModified = false;
                        break;
                }

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        break;
                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Disable;
                        auditEntry.OldValues[propertyName] = property.OriginalValue!;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            if (entry.Entity is Entity && propertyName == "IsDisabled" && property.IsModified &&
                                !(bool)property.OriginalValue! &&
                                (bool)property.CurrentValue!) auditEntry.AuditType = AuditType.Disable;
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }

                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries) AuditLogs.Add(auditEntry.ToAudit());
    }
}