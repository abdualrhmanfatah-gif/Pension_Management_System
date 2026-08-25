using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Inventory;

namespace PensionMS.Infrastructure.Data.Configurations.Inventory;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(e => e.InventoryItemID);

        builder.Property(e => e.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Unit).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.SerialNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.Property(e => e.Condition)
            .HasConversion<int>()
            .HasDefaultValue(ItemCondition.New);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .HasDefaultValue(ItemStatus.Active);

        builder.HasIndex(e => e.ItemCode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
