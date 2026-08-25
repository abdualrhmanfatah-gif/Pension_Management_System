using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class FixedAssetCategoryConfiguration : IEntityTypeConfiguration<FixedAssetCategory>
{
    public void Configure(EntityTypeBuilder<FixedAssetCategory> builder)
    {
        builder.ToTable("FixedAssetCategories");
        builder.HasKey(e => e.CategoryID);
        builder.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CategoryCode).HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.SalvagePercent).HasPrecision(5, 2);
        builder.HasIndex(e => e.CategoryName).IsUnique();
        builder.HasIndex(e => e.CategoryCode).IsUnique().HasFilter("[CategoryCode] IS NOT NULL");

        builder.HasOne(e => e.AssetType)
            .WithMany(t => t.Categories)
            .HasForeignKey(e => e.AssetTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssetAccount).WithMany().HasForeignKey(e => e.AssetAccountID).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.AccumDeprAccount).WithMany().HasForeignKey(e => e.AccumDeprAccountID).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.DeprExpenseAccount).WithMany().HasForeignKey(e => e.DeprExpenseAccountID).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
