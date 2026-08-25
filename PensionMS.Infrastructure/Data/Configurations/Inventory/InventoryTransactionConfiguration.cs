using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Inventory;

namespace PensionMS.Infrastructure.Data.Configurations.Inventory;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(e => e.InventoryTransactionID);

        builder.Property(e => e.InvoiceNumber).HasMaxLength(100);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.Reason).HasMaxLength(500);

        builder.Property(e => e.UnitCost).HasPrecision(18, 2);
        builder.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("SAR");
        builder.Property(e => e.WithdrawalOrderNumber).HasMaxLength(100);

        builder.Property(e => e.TransactionType)
            .HasConversion<int>();

        builder.HasOne(e => e.InventoryItem)
            .WithMany(i => i.Transactions)
            .HasForeignKey(e => e.InventoryItemID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PerformedByUser).WithMany().HasForeignKey(e => e.PerformedByUserID).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.BatchID);
    }
}
