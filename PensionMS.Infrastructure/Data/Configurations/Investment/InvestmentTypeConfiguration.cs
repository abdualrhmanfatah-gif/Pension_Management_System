using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class InvestmentTypeConfiguration : IEntityTypeConfiguration<InvestmentType>
{
    public void Configure(EntityTypeBuilder<InvestmentType> builder)
    {
        builder.ToTable("InvestmentTypes");
        builder.HasKey(e => e.TypeID);
        builder.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.HasIndex(e => new { e.TypeName, e.Code }).IsUnique();

        builder.HasData(
            new InvestmentType { TypeID = 1, TypeName = "عقاري", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new InvestmentType { TypeID = 2, TypeName = "أسهم", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new InvestmentType { TypeID = 3, TypeName = "سندات", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new InvestmentType { TypeID = 4, TypeName = "وديعة بنكية", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new InvestmentType { TypeID = 5, TypeName = "عملات", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
