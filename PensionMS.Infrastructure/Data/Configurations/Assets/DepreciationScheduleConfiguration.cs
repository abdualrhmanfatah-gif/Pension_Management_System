using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class DepreciationScheduleConfiguration : IEntityTypeConfiguration<DepreciationSchedule>
{
    public void Configure(EntityTypeBuilder<DepreciationSchedule> builder)
    {
        builder.ToTable("DepreciationSchedules");
        builder.HasKey(e => e.ScheduleID);
        builder.Property(e => e.DepreciationAmount).HasPrecision(18, 2);
        builder.Property(e => e.AccumulatedDepreciation).HasPrecision(18, 2);
        builder.Property(e => e.NetBookValue).HasPrecision(18, 2);
        builder.HasIndex(e => new { e.AssetID, e.PeriodYear, e.PeriodMonth }).IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany(a => a.DepreciationSchedules)
            .HasForeignKey(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
