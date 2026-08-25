using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests", table =>
        {
            table.HasCheckConstraint("CK_LeaveRequests_DateRange", "[EndDate] >= [StartDate]");
            table.HasCheckConstraint("CK_LeaveRequests_Days", "[RequestedDays] >= 0");
        });

        builder.HasKey(e => e.LeaveRequestID);

        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.Reason).HasMaxLength(1000);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeaveType)
            .WithMany(t => t.LeaveRequests)
            .HasForeignKey(e => e.LeaveTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedByUserID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmployeeID, e.StartDate, e.EndDate });
        
        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
