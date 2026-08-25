using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class AdministrativeOrderTypeConfiguration : IEntityTypeConfiguration<AdministrativeOrderType>
{
    public void Configure(EntityTypeBuilder<AdministrativeOrderType> builder)
    {
        builder.ToTable("AdministrativeOrderTypes");
        builder.HasKey(t => t.AdministrativeOrderTypeID);

        builder.Property(t => t.Code).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => t.Code).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasData(
            new AdministrativeOrderType { AdministrativeOrderTypeID = 1, Code = "ORD_APP", Name = "قرار تعيين", Description = "قرار تعيين موظف جديد", GeneratesServiceHistory = true },
            new AdministrativeOrderType { AdministrativeOrderTypeID = 2, Code = "ORD_PRO", Name = "قرار ترقية", Description = "قرار ترقية رتبة أو درجة وظيفية", GeneratesServiceHistory = true },
            new AdministrativeOrderType { AdministrativeOrderTypeID = 3, Code = "ORD_TRA", Name = "قرار نقل", Description = "قرار نقل بين الإدارات أو الأقسام", GeneratesServiceHistory = true },
            new AdministrativeOrderType { AdministrativeOrderTypeID = 4, Code = "ORD_DIS", Name = "قرار تأديبي", Description = "قرار تأديبي أو لفت نظر", GeneratesServiceHistory = true },
            new AdministrativeOrderType { AdministrativeOrderTypeID = 5, Code = "ORD_TER", Name = "قرار إنهاء خدمة", Description = "قرار إنهاء خدمة موظف", GeneratesServiceHistory = true }
        );
    }
}
