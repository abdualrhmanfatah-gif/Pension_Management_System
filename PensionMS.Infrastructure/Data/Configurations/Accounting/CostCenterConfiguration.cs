using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");
        builder.HasKey(e => e.CostCenterID);

        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => e.Code).IsUnique();

        // Self-referencing hierarchy
        builder.HasOne(e => e.ParentCostCenter)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentCostCenterID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
