using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class EmploymentStatusConfiguration : IEntityTypeConfiguration<EmploymentStatus>
{
    public void Configure(EntityTypeBuilder<EmploymentStatus> builder)
    {
        builder.ToTable("EmploymentStatuses");
        builder.HasKey(e => e.EmploymentStatusID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new EmploymentStatus { EmploymentStatusID = 1, Code = "ACTIVE", Name = "مستمر بالعمل", IsActive = true, IsInService = true, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 2, Code = "UNPAID_LEAVE", Name = "إجازة بدون معاش", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 3, Code = "SECONDED", Name = "منتدب / مُعار", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 4, Code = "SUSPENDED", Name = "موقوف عن العمل", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 5, Code = "ABSENT", Name = "منقطع عن العمل", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 6, Code = "RESIGNED", Name = "مستقيل", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 7, Code = "TERMINATED", Name = "مفصول", IsActive = false, IsInService = false, CreatedAt = seedDate },
            new EmploymentStatus { EmploymentStatusID = 8, Code = "RETIRED_HR", Name = "مُحال للتقاعد (إدارياً)", IsActive = false, IsInService = false, CreatedAt = seedDate }
        );
    }
}
