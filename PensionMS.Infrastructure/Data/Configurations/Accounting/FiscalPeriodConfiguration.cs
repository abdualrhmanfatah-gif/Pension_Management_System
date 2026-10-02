using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");
        builder.HasKey(e => e.PeriodID);

        builder.Property(e => e.PeriodName).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => new { e.FiscalYearID, e.PeriodNumber }).IsUnique();

        builder.HasOne(e => e.FiscalYear)
            .WithMany(fy => fy.FiscalPeriods)
            .HasForeignKey(e => e.FiscalYearID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Quarter)
            .WithMany(q => q.FiscalPeriods)
            .HasForeignKey(e => e.QuarterID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ClosedByUser)
            .WithMany()
            .HasForeignKey(e => e.ClosedByUserID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
