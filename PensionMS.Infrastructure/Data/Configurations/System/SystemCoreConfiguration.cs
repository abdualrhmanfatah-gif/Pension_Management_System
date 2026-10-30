using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.System;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.LogID);
        builder.Property(e => e.Action).HasMaxLength(100);
<<<<<<< HEAD
        builder.HasIndex(e => new { e.TableID, e.RecordID, e.CreatedAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("IX_AuditLogs_Record_CreatedAt");
        builder.HasIndex(e => new { e.CreatedAt, e.LogID })
            .IsDescending(true, true)
            .IncludeProperties(e => new { e.TableID, e.UserID, e.Action })
            .HasDatabaseName("IX_AuditLogs_Recent");
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }
}

public class ImmutableAuditLogConfiguration : IEntityTypeConfiguration<ImmutableAuditLog>
{
    public void Configure(EntityTypeBuilder<ImmutableAuditLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasMaxLength(256);
        builder.Property(e => e.ActorName).HasMaxLength(256);
        builder.Property(e => e.Action).HasMaxLength(100);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.NotificationID);
        builder.Property(e => e.Type).HasMaxLength(100);
        builder.Property(e => e.Category).HasMaxLength(100);

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Actor)
            .WithMany(u => u.TriggeredNotifications)
            .HasForeignKey(n => n.ActorID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.RelatedTable)
            .WithMany(t => t.Notifications)
            .HasForeignKey(n => n.RelatedTableID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SyncQueueConfiguration : IEntityTypeConfiguration<SyncQueue>
{
    public void Configure(EntityTypeBuilder<SyncQueue> builder)
    {
        builder.HasKey(e => e.SyncID);
        builder.Property(e => e.Status).HasMaxLength(100);
        builder.Property(e => e.Action).HasMaxLength(50);
        builder.Property(e => e.DeviceID).HasMaxLength(450);

        builder.HasOne(s => s.DeviceRegistration)
            .WithMany(d => d.SyncQueues)
            .HasForeignKey(s => s.DeviceID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SystemTable)
            .WithMany(t => t.SyncQueues)
            .HasForeignKey(s => s.TableID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SystemExtraConfiguration : IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> builder)
    {
        builder.HasKey(e => e.DeviceID);
    }
}

public class FileSnapshotConfiguration : IEntityTypeConfiguration<FileSnapshot>
{
    public void Configure(EntityTypeBuilder<FileSnapshot> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(fs => new { fs.FileId, fs.Version }).IsUnique();
    }
}
