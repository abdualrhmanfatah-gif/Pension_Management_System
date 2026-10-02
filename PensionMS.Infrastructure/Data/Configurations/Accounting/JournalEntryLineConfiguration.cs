using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(e => e.LineID);

        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => new { e.JournalEntryID });

        builder.HasOne(e => e.JournalEntry)
            .WithMany(e => e.Lines)
            .HasForeignKey(e => e.JournalEntryID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
