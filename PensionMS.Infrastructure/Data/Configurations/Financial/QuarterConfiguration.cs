using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Infrastructure.Data.Configurations.Financial;

public class QuarterConfiguration : IEntityTypeConfiguration<Quarter>
{
    public void Configure(EntityTypeBuilder<Quarter> builder)
    {
        builder.ToTable("Quarters");
        builder.HasKey(e => e.QuarterID);

        builder.Property(e => e.FiscalYearID).IsRequired();
        // QuarterNumber 1-4 enforced by [Range(1,4)] on entity
        builder.Property(e => e.IsClosed).IsRequired().HasDefaultValue(false);

        builder.HasOne(e => e.FiscalYear)
            .WithMany(fy => fy.Quarters)
            .HasForeignKey(e => e.FiscalYearID)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index: one quarter number per fiscal year
        builder.HasIndex(e => new { e.FiscalYearID, e.QuarterNumber }).IsUnique();
    }
}
