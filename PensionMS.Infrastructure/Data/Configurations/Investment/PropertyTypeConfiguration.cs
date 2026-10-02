using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class PropertyTypeConfiguration : IEntityTypeConfiguration<PropertyType>
{
    public void Configure(EntityTypeBuilder<PropertyType> builder)
    {
        builder.ToTable("PropertyTypes");
        builder.HasKey(e => e.TypeID);
        builder.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.TypeName).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasData(
            new PropertyType { TypeID = 1, TypeName = "أرض", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PropertyType { TypeID = 2, TypeName = "مبنى سكني", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PropertyType { TypeID = 3, TypeName = "مجمع تجاري", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PropertyType { TypeID = 4, TypeName = "مستودع", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
