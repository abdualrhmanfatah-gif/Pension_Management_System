using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.RoleID);

        builder.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasData(
            new Role { RoleID = 1, RoleName = "Admin",          Description = "مدير النظام — الحاسب الآلي وصلاحيات كاملة",     IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 2, RoleName = "PensionManager", Description = "مدير إدارة المعاشات — مراجعة واعتماد الملفات",    IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 3, RoleName = "PensionClerk",   Description = "موظف المعاشات — إنشاء واستكمال الملفات",          IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 4, RoleName = "FinanceOfficer", Description = "الموظف المالي — إدارة المعاشات والتسويات المالية", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 5, RoleName = "Auditor",        Description = "المدقق — قراءة ومراجعة فقط لجميع البيانات",         IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 6, RoleName = "Archivist",      Description = "موظف الأرشيف — رفع وإدارة الوثائق فقط",             IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleID = 7, RoleName = "Technician",     Description = "فني استثمار — عرض بيانات الصيانة والوثائق",         IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
