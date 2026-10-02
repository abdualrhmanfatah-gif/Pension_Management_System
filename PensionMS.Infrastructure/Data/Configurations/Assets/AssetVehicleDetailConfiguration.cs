using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class AssetVehicleDetailConfiguration : IEntityTypeConfiguration<AssetVehicleDetail>
{
    public void Configure(EntityTypeBuilder<AssetVehicleDetail> builder)
    {
        builder.ToTable("AssetVehicleDetails");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.PlateNumber).HasMaxLength(50);
        builder.Property(e => e.PlateLetter1).HasMaxLength(10);
        builder.Property(e => e.PlateLetter2).HasMaxLength(10);
        builder.Property(e => e.PlateLetter3).HasMaxLength(10);
        builder.Property(e => e.EngineNumber).HasMaxLength(100);
        builder.Property(e => e.ChassisNumber).HasMaxLength(100);
        builder.Property(e => e.TransmissionType).HasMaxLength(50);
        builder.Property(e => e.FuelType).HasMaxLength(50);
        builder.Property(e => e.EngineCapacity).HasMaxLength(50);
        builder.Property(e => e.TireSize).HasMaxLength(50);
        builder.Property(e => e.Color).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Asset)
            .WithOne(a => a.VehicleDetail)
            .HasForeignKey<AssetVehicleDetail>(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
