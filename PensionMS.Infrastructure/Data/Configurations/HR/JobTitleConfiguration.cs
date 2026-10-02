using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class JobTitleConfiguration : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> builder)
    {
        builder.ToTable("JobTitles");
        builder.HasKey(e => e.JobTitleID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            // ─── قيادي — مادة (3)، مادة (5)، مادة (6) ───
            new JobTitle { JobTitleID = 1, Code = "DIR_GEN", Name = "المدير العام", Description = "قيادي", IsActive = true, CreatedAt = seedDate },
            new JobTitle { JobTitleID = 2, Code = "DEP_DIR_GEN", Name = "نائب المدير العام", Description = "قيادي", IsActive = true, CreatedAt = seedDate },
            
            // ─── إشرافي — مادة (4)، مادة (12) ───
            new JobTitle { JobTitleID = 3, Code = "DEPT_MGR", Name = "مدير إدارة", Description = "إشرافي", IsActive = true, CreatedAt = seedDate },
            new JobTitle { JobTitleID = 4, Code = "DG_OFFICE_MGR", Name = "مدير مكتب المدير العام", Description = "إشرافي", IsActive = true, CreatedAt = seedDate },
            new JobTitle { JobTitleID = 5, Code = "SEC_HEAD", Name = "رئيس قسم", Description = "إشرافي", IsActive = true, CreatedAt = seedDate },
            
            // ─── تخصصي وإداري — العاملون ───
            new JobTitle { JobTitleID = 6, Code = "EMPLOYEE", Name = "موظف", Description = "تخصصي وإداري", IsActive = true, CreatedAt = seedDate }
        );
    }
}
