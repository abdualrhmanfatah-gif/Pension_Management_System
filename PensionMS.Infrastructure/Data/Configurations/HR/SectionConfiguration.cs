using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");
        builder.HasKey(e => e.SectionID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Sections)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => new { e.DepartmentID, e.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            // ─── إدارة المعاشات  (Dept 1) ───
            new Section { SectionID = 1, DepartmentID = 1, Code = "PEN_RESEARCH", Name = "قسم البحث الاجتماعي والفحص", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 2, DepartmentID = 1, Code = "PEN_SETTLE", Name = "قسم التسوية والربط", IsActive = true, CreatedAt = seedDate },
<<<<<<< HEAD
            new Section { SectionID = 3, DepartmentID = 1, Code = "PEN_ENTITLE", Name = "قسم الاستحقاق", IsActive = true, CreatedAt = seedDate },
=======
            new Section { SectionID = 3, DepartmentID = 1, Code = "PEN_ENTITLE", Name = "قسم الاستحقاقات", IsActive = true, CreatedAt = seedDate },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            new Section { SectionID = 4, DepartmentID = 1, Code = "PEN_RECORDS", Name = "قسم السجلات والأحصاء", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 5, DepartmentID = 1, Code = "PEN_IT", Name = "قسم الحاسب الآلي", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 6, DepartmentID = 1, Code = "PEN_DOCS", Name = "قسم التوثيق", IsActive = true, CreatedAt = seedDate },
            
            // ─── إدارة الشئون الإدارية (Dept 2) ───
            new Section { SectionID = 7, DepartmentID = 2, Code = "ADM_SECRET", Name = "قسم السكرتارية", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 8, DepartmentID = 2, Code = "ADM_PRINT", Name = "قسم الطباعة والنسخ", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 9, DepartmentID = 2, Code = "ADM_FINANCE", Name = "قسم الشئون المالية والإدارية", IsActive = true, CreatedAt = seedDate },
            
            // ─── إدارة الاستثمار (Dept 3) ───
            new Section { SectionID = 10, DepartmentID = 3, Code = "INV_STUDIES", Name = "قسم الدراسات والجدوى الاقتصادي", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 11, DepartmentID = 3, Code = "INV_DESIGN", Name = "قسم التصاميم", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 12, DepartmentID = 3, Code = "INV_TRADE", Name = "قسم العلاقات التجارية", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 13, DepartmentID = 3, Code = "INV_EXEC", Name = "قسم التنفيذ والمتابعة", IsActive = true, CreatedAt = seedDate },
            
            // ─── إدارة الشئون المالية (Dept 4) ───
            new Section { SectionID = 14, DepartmentID = 4, Code = "FIN_BUDGET", Name = "قسم الميزانية والحسابات", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 15, DepartmentID = 4, Code = "FIN_REVENUE", Name = "قسم الإيرادات", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 16, DepartmentID = 4, Code = "FIN_EXPENSES", Name = "قسم المصروفات والمدفوعات", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 17, DepartmentID = 4, Code = "FIN_AUDIT", Name = "قسم المراجعة والرقابة المالية", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 18, DepartmentID = 4, Code = "FIN_PURCH", Name = "قسم المشتروات والمخازن", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 19, DepartmentID = 4, Code = "FIN_TREASURY", Name = "قسم الخزينة", IsActive = true, CreatedAt = seedDate },
            
            // ─── الإدارة القانونية (Dept 5) ───
            new Section { SectionID = 20, DepartmentID = 5, Code = "LEG_CONSULT", Name = "قسم الاستشارات القانونية", IsActive = true, CreatedAt = seedDate },
            new Section { SectionID = 21, DepartmentID = 5, Code = "LEG_INVEST", Name = "قسم التحقيقات والمرافعات", IsActive = true, CreatedAt = seedDate }
        );
    }
}
