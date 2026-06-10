using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yanga.Module.EntityFrameworkCore.AuditTrail.Enums;

namespace ElectroTech.Infrastructure.DbContexts
{
    public abstract class AuditableLogContext : DbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }

        public AuditableLogContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual async Task<int> SaveChangesAsync(int? ComId = null, string userId = null)
        {
            List<AuditEntryLog> auditEntries = OnBeforeSaveChanges(ComId, userId);
            int result = await base.SaveChangesAsync();
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntryLog> OnBeforeSaveChanges(int? ComId, string userId)
        {
            ChangeTracker.DetectChanges();
            List<AuditEntryLog> list = new List<AuditEntryLog>();
            foreach (EntityEntry item in ChangeTracker.Entries())
            {
                if (item.Entity is AuditLog || item.State == EntityState.Detached || item.State == EntityState.Unchanged)
                {
                    continue;
                }

                AuditEntryLog auditEntry = new AuditEntryLog(item);
                auditEntry.TableName = item.Entity.GetType().Name;
                auditEntry.ComId = ComId ?? 0;
                auditEntry.UserId = userId;
                list.Add(auditEntry);
                foreach (PropertyEntry property in item.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string name = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[name] = property.CurrentValue;
                        continue;
                    }

                    switch (item.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[name] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[name] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(name);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[name] = property.OriginalValue;
                                auditEntry.NewValues[name] = property.CurrentValue;
                            }

                            break;
                    }
                }
            }

            foreach (AuditEntryLog item2 in list.Where((AuditEntryLog _) => !_.HasTemporaryProperties))
            {
                AuditLogs.Add(item2.ToAudit());
            }

            return list.Where((AuditEntryLog _) => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntryLog> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
            {
                return Task.CompletedTask;
            }

            foreach (AuditEntryLog auditEntry in auditEntries)
            {
                foreach (PropertyEntry temporaryProperty in auditEntry.TemporaryProperties)
                {
                    if (temporaryProperty.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[temporaryProperty.Metadata.Name] = temporaryProperty.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[temporaryProperty.Metadata.Name] = temporaryProperty.CurrentValue;
                    }
                }

                AuditLogs.Add(auditEntry.ToAudit());
            }

            return SaveChangesAsync();
        }
        public class AuditEntryLog
        {
            public EntityEntry Entry { get; }

            public int ComId { get; set; }
            public string UserId { get; set; }

            public string TableName { get; set; }

            public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();


            public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();


            public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();


            public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();


            public AuditType AuditType { get; set; }

            public List<string> ChangedColumns { get; } = new List<string>();


            public bool HasTemporaryProperties => TemporaryProperties.Any();

            public AuditEntryLog(EntityEntry entry)
            {
                Entry = entry;
            }

            public AuditLog ToAudit()
            {
                AuditLog audit = new AuditLog();
                audit.UserId = UserId;
                audit.ComId = ComId;
                audit.Type = AuditType.ToString();
                audit.TableName = TableName;
                audit.DateTime = DateTime.UtcNow;
                audit.PrimaryKey = JsonConvert.SerializeObject(KeyValues);
                audit.OldValues = ((OldValues.Count == 0) ? null : JsonConvert.SerializeObject(OldValues));
                audit.NewValues = ((NewValues.Count == 0) ? null : JsonConvert.SerializeObject(NewValues));
                audit.AffectedColumns = ((ChangedColumns.Count == 0) ? null : JsonConvert.SerializeObject(ChangedColumns));
                return audit;
            }
        }
    }
}
