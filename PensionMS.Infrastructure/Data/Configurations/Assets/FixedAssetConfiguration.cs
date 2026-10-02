using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class FixedAssetConfiguration : IEntityTypeConfiguration<FixedAsset>
{
    public void Configure(EntityTypeBuilder<FixedAsset> builder)
    {
        builder.ToTable("FixedAssets");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.AssetNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.AssetName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.SerialNumber).HasMaxLength(100);
        builder.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.DisposalReason).HasMaxLength(500);
        builder.Property(e => e.AcquiredCost).HasPrecision(18, 2);
        builder.Property(e => e.SalvageValue).HasPrecision(18, 2);
        builder.Property(e => e.AccumulatedDepreciation).HasPrecision(18, 2);
        builder.Property(e => e.NetBookValue).HasPrecision(18, 2);
        builder.HasIndex(e => e.AssetNumber).IsUnique();
        builder.HasIndex(e => e.SerialNumber).HasFilter("[SerialNumber] IS NOT NULL");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Assets)
            .HasForeignKey(e => e.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssetType)
            .WithMany(t => t.Assets)
            .HasForeignKey(e => e.AssetTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CustodianEmployee).WithMany().HasForeignKey(e => e.CustodianEmployeeID).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.AssetAccount).WithMany().HasForeignKey(e => e.AssetAccountID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.AccumDeprAccount).WithMany().HasForeignKey(e => e.AccumDeprAccountID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DeprExpenseAccount).WithMany().HasForeignKey(e => e.DeprExpenseAccountID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
