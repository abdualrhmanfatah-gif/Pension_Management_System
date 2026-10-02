using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(e => e.LeaveTypeID);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new LeaveType { LeaveTypeID = 1, Code = "ANNUAL", Name = "إجازة اعتيادية", Description = "إجازة اعتيادية منتظمة.", IsActive = true, IsPaid = true, CountsTowardsService = true, MaxDaysPerRequest = 30, CreatedAt = seedDate },
            new LeaveType { LeaveTypeID = 2, Code = "SICK", Name = "إجازة مرضية", Description = "إجازة مرضية معتمدة.", IsActive = true, IsPaid = true, CountsTowardsService = true, MaxDaysPerRequest = 30, CreatedAt = seedDate },
            new LeaveType { LeaveTypeID = 3, Code = "UNPAID", Name = "إجازة بدون معاش", Description = "إجازة بدون معاش.", IsActive = true, IsPaid = false, CountsTowardsService = false, MaxDaysPerRequest = 90, CreatedAt = seedDate },
            new LeaveType { LeaveTypeID = 4, Code = "STUDY", Name = "إجازة دراسية", Description = "إجازة دراسية معتمدة.", IsActive = true, IsPaid = true, CountsTowardsService = true, MaxDaysPerRequest = 365, CreatedAt = seedDate }
        );
    }
}
