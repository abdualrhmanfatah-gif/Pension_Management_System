using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class AssetLandDetailConfiguration : IEntityTypeConfiguration<AssetLandDetail>
{
    public void Configure(EntityTypeBuilder<AssetLandDetail> builder)
    {
        builder.ToTable("AssetLandDetails");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.PlotNumber).HasMaxLength(100);
        builder.Property(e => e.LandAreaSqm).HasPrecision(12, 2);
        builder.Property(e => e.OwnershipDocNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Asset)
            .WithOne(a => a.LandDetail)
            .HasForeignKey<AssetLandDetail>(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
