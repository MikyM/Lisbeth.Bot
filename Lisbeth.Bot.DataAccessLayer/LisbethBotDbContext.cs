using Lisbeth.Bot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MikyM.Common.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Lisbeth.Bot.DataAccessLayer
{
    public sealed class LisbethBotDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public LisbethBotDbContext(DbContextOptions<LisbethBotDbContext> options) : base(options)
        {
            ChangeTracker.StateChanged += OnEntityStateChanged;
        }

        public DbSet<Mute> Mutes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<ServerBooster> ServerBoosters { get; set; }
        public DbSet<Audit> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // fluent api to do
            base.OnModelCreating(modelBuilder);
        }

        private static void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
        {
/*            if (e.NewState is EntityState.Modified && e.Entry.Entity is Entity)
            {
                ((Entity)e.Entry.Entity).UpdatedAt = DateTime.UtcNow;
            }*/
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
                if (entry.Entity is Audit || entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry) {TableName = entry.Entity.GetType().Name, UserId = userId};

                auditEntries.Add(auditEntry);

                if(entry.Entity is Entity entity)
                {
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
                                if (entry.Entity is Entity && propertyName == "IsDisabled" && property.IsModified)
                                {
                                    if (!(bool)property.OriginalValue && (bool)property.CurrentValue)
                                    {
                                        auditEntry.AuditType = AuditType.Disable;
                                    }
                                    else
                                    {
                                        auditEntry.AuditType = AuditType.Update;
                                    }
                                }
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }
    }
}
