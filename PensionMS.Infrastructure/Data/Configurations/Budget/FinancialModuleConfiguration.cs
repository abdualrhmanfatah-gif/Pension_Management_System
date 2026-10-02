using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Infrastructure.Data.Configurations.Budget;

public class QuarterConfiguration : IEntityTypeConfiguration<Quarter>
{
    public void Configure(EntityTypeBuilder<Quarter> builder)
    {
        builder.ToTable("Quarters");
        builder.HasKey(e => e.QuarterID);

        builder.HasOne(e => e.FiscalYear)
            .WithMany(f => f.Quarters)
            .HasForeignKey(e => e.FiscalYearID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
