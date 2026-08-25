using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(e => e.DepartmentID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Department { DepartmentID = 1, Code = "PENSIONS", Name = "إدارة المعاشات ", IsActive = true, CreatedAt = seedDate },
            new Department { DepartmentID = 2, Code = "ADMIN", Name = "إدارة الشئون الإدارية", IsActive = true, CreatedAt = seedDate },
            new Department { DepartmentID = 3, Code = "INVEST", Name = "إدارة الاستثمار", IsActive = true, CreatedAt = seedDate },
            new Department { DepartmentID = 4, Code = "FINANCE", Name = "إدارة الشئون المالية", IsActive = true, CreatedAt = seedDate },
            new Department { DepartmentID = 5, Code = "LEGAL", Name = "الإدارة القانونية", IsActive = true, CreatedAt = seedDate },
            new Department { DepartmentID = 6, Code = "DG_OFFICE", Name = "مكتب المدير العام", IsActive = true, CreatedAt = seedDate }
        );
    }
}
