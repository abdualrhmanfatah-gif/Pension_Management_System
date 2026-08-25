using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class ServiceHistoryEventConfiguration : IEntityTypeConfiguration<ServiceHistoryEvent>
{
    public void Configure(EntityTypeBuilder<ServiceHistoryEvent> builder)
    {
        builder.ToTable("ServiceHistoryEvents");
        builder.HasKey(e => e.ServiceHistoryEventID);

        builder.Property(e => e.EventType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(1000);
        builder.Property(e => e.SourceEntityType).HasMaxLength(100);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.ServiceHistoryEvents)
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmployeeID, e.EventDate });
    }
}
