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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.DataAccessLayer
{
    public sealed class LisbethBotDbContext : DbContext
    {
        public LisbethBotDbContext(DbContextOptions<LisbethBotDbContext> options) : base(options)
        {
        }

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Mute> Mutes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<ServerBooster> ServerBoosters { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Prune> Prunes { get; set; }
        public DbSet<TicketingConfig> TicketingConfigs { get; set; }
        public DbSet<ModerationConfig> ModerationConfigs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecurringReminder> RecurringReminders { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<EmbedConfig> EmbedConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LisbethBotDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges("placeholder");
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry) {TableName = entry.Entity.GetType().Name, UserId = userId};

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
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Disable;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                if (entry.Entity is Entity && propertyName == "IsDisabled" && property.IsModified &&
                                    !(bool) property.OriginalValue &&
                                    (bool) property.CurrentValue) auditEntry.AuditType = AuditType.Disable;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }

                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries) AuditLogs.Add(auditEntry.ToAudit());
        }
    }
}