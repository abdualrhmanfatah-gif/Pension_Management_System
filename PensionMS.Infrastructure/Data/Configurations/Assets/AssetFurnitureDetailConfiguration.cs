using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class AssetFurnitureDetailConfiguration : IEntityTypeConfiguration<AssetFurnitureDetail>
{
    public void Configure(EntityTypeBuilder<AssetFurnitureDetail> builder)
    {
        builder.ToTable("AssetFurnitureDetails");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.Material).HasMaxLength(50);
        builder.Property(e => e.Color).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Asset)
            .WithOne(a => a.FurnitureDetail)
            .HasForeignKey<AssetFurnitureDetail>(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
