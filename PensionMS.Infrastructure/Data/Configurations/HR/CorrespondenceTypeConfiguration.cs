using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class CorrespondenceTypeConfiguration : IEntityTypeConfiguration<CorrespondenceType>
{
    public void Configure(EntityTypeBuilder<CorrespondenceType> builder)
    {
        builder.ToTable("CorrespondenceTypes");
        builder.HasKey(e => e.CorrespondenceTypeID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();

        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new CorrespondenceType { CorrespondenceTypeID = 1, Code = "INTERNAL", Name = "مراسلات الوزارة (الداخلي)", IsActive = true, CreatedAt = seedDate },
            new CorrespondenceType { CorrespondenceTypeID = 2, Code = "EXTERNAL", Name = "مراسلات (الخارجي)", IsActive = true, CreatedAt = seedDate },
            new CorrespondenceType { CorrespondenceTypeID = 3, Code = "ADMIN_ORDER", Name = "أوامر إدارية", IsActive = true, CreatedAt = seedDate },
            new CorrespondenceType { CorrespondenceTypeID = 4, Code = "INTERNAL_DIRECTIVE", Name = "توجيهات وتعاميم داخلية", IsActive = true, CreatedAt = seedDate },
            new CorrespondenceType { CorrespondenceTypeID = 5, Code = "EXTERNAL_DIRECTIVE", Name = "توجيهات وتعاميم خارجية", IsActive = true, CreatedAt = seedDate }
        );
    }
}
