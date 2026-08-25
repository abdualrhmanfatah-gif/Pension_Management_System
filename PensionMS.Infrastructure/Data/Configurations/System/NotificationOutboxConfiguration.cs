using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.System;

public class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.HasKey(e => e.OutboxId);

        builder.Property(e => e.Channel).HasColumnType("tinyint");
        builder.Property(e => e.Status).HasColumnType("tinyint");
        builder.Property(e => e.ErrorMessage).HasMaxLength(500);

        builder.HasOne(e => e.Notification)
            .WithMany()
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.Status, e.RetryCount });
    }
}
