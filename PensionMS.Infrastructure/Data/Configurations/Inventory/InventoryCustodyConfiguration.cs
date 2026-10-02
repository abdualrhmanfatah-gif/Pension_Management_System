using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Inventory;

namespace PensionMS.Infrastructure.Data.Configurations.Inventory;

public class InventoryCustodyConfiguration : IEntityTypeConfiguration<InventoryCustody>
{
    public void Configure(EntityTypeBuilder<InventoryCustody> builder)
    {
        builder.ToTable("InventoryCustodies");
        builder.HasKey(e => e.InventoryCustodyID);

        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .HasDefaultValue(CustodyStatus.Active);

        builder.HasOne(e => e.Asset)
            .WithMany(a => a.Custodies)
            .HasForeignKey(e => e.AssetID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssignedByUser).WithMany().HasForeignKey(e => e.AssignedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReturnedToUser).WithMany().HasForeignKey(e => e.ReturnedToUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
