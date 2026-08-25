using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class AssetBuildingDetailConfiguration : IEntityTypeConfiguration<AssetBuildingDetail>
{
    public void Configure(EntityTypeBuilder<AssetBuildingDetail> builder)
    {
        builder.ToTable("AssetBuildingDetails");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.AreaSqm).HasPrecision(10, 2);
        builder.Property(e => e.BuiltAreaSqm).HasPrecision(10, 2);
        builder.Property(e => e.OwnershipDocNumber).HasMaxLength(100);
        builder.Property(e => e.LicenseNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Asset)
            .WithOne(a => a.BuildingDetail)
            .HasForeignKey<AssetBuildingDetail>(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
