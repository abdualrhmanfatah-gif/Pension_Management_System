using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Infrastructure.Data.Configurations.Assets;

public class FixedAssetTypeConfiguration : IEntityTypeConfiguration<FixedAssetType>
{
    public void Configure(EntityTypeBuilder<FixedAssetType> builder)
    {
        builder.ToTable("FixedAssetTypes");
        builder.HasKey(e => e.AssetTypeID);
        builder.Property(e => e.TypeName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TypeCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(200);
        builder.HasIndex(e => e.TypeName).IsUnique();
        builder.HasIndex(e => e.TypeCode).IsUnique();
    }
}
