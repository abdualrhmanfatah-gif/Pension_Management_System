using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class AssetITDetailConfiguration : IEntityTypeConfiguration<AssetITDetail>
{
    public void Configure(EntityTypeBuilder<AssetITDetail> builder)
    {
        builder.ToTable("AssetITDetails");
        builder.HasKey(e => e.AssetID);
        builder.Property(e => e.Model).HasMaxLength(100);
        builder.Property(e => e.Manufacturer).HasMaxLength(100);
        builder.Property(e => e.ProcessorType).HasMaxLength(100);
        builder.Property(e => e.ProcessorSpeed).HasMaxLength(50);
        builder.Property(e => e.RamSize).HasMaxLength(50);
        builder.Property(e => e.HardDiskType).HasMaxLength(50);
        builder.Property(e => e.HardDiskSize).HasMaxLength(50);
        builder.Property(e => e.GpuModel).HasMaxLength(100);
        builder.Property(e => e.ScreenSize).HasMaxLength(50);
        builder.Property(e => e.ScreenResolution).HasMaxLength(50);
        builder.Property(e => e.OperatingSystem).HasMaxLength(50);
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.MacAddress).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Asset)
            .WithOne(a => a.ITDetail)
            .HasForeignKey<AssetITDetail>(e => e.AssetID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
