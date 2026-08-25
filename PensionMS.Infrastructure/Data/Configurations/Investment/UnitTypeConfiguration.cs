using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.ToTable("UnitTypes");
        builder.HasKey(e => e.TypeID);
        builder.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.TypeName).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasData(
            new UnitType { TypeID = 1, TypeName = "شقة", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UnitType { TypeID = 2, TypeName = "مكتب", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UnitType { TypeID = 3, TypeName = "معرض تجاري", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UnitType { TypeID = 4, TypeName = "مستودع فرعي", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
