using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(e => e.JournalEntryID);

        builder.Property(e => e.EntryNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.EntryNumber).IsUnique();
        builder.HasIndex(e => new { e.PeriodID, e.Status });
        builder.HasIndex(e => e.JournalEntryTypeID);

        builder.HasOne(e => e.JournalEntryType)
            .WithMany(e => e.JournalEntries)
            .HasForeignKey(e => e.JournalEntryTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Period)
            .WithMany()
            .HasForeignKey(e => e.PeriodID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PostedByUser)
            .WithMany()
            .HasForeignKey(e => e.PostedByUserID)
            .OnDelete(DeleteBehavior.SetNull);

        // Self-referencing for reversals
        builder.HasOne(e => e.ReversedEntry)
            .WithMany(e => e.ReverseEntries)
            .HasForeignKey(e => e.ReversedEntryID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
