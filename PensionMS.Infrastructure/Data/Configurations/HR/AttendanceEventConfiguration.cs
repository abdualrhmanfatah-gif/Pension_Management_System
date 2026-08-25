using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class AttendanceEventConfiguration : IEntityTypeConfiguration<AttendanceEvent>
{
    public void Configure(EntityTypeBuilder<AttendanceEvent> builder)
    {
        builder.ToTable("AttendanceEvents");
        builder.HasKey(e => e.AttendanceEventID);

        builder.Property(e => e.EventType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.AttendanceEvents)
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RecordedByUser)
            .WithMany()
            .HasForeignKey(e => e.RecordedByUserID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmployeeID, e.EventDate, e.EventType });
    }
}
