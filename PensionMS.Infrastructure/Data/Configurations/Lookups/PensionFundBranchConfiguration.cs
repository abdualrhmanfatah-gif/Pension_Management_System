using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class PensionFundBranchConfiguration : IEntityTypeConfiguration<PensionFundBranch>
{
    public void Configure(EntityTypeBuilder<PensionFundBranch> builder)
    {
        builder.HasKey(e => e.PensionBranchID);

        builder.Property(e => e.BranchName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Governorate).HasMaxLength(100);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.ManagerName).HasMaxLength(200);
        builder.Property(e => e.ManagerPhone).HasMaxLength(20);

        builder.HasData(
            new PensionFundBranch { PensionBranchID = 1, BranchName = "الادارة العامة", Governorate = "عدن", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PensionFundBranch { PensionBranchID = 2, BranchName = "فرع صنعاء", ParentBranchId = 1, Governorate = "صنعاء", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PensionFundBranch { PensionBranchID = 3, BranchName = "فرع تعز", ParentBranchId = 1, Governorate = "تعز", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PensionFundBranch { PensionBranchID = 4, BranchName = "فرع حضرموت", Governorate = "حضرموت", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PensionFundBranch { PensionBranchID = 5, BranchName = "فرع الحديدة", Governorate = "الحديدة", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
