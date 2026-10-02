using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Infrastructure.Data.Configurations.Budget;

public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        builder.ToTable("FiscalYears");
        builder.HasKey(e => e.FiscalYearID);
        builder.Property(e => e.YearName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.TotalBudget).HasPrecision(18, 2);
    }
}
