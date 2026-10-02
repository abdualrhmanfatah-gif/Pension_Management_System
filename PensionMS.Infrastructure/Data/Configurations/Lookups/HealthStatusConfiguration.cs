using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class HealthStatusConfiguration : IEntityTypeConfiguration<HealthStatus>
{
    public void Configure(EntityTypeBuilder<HealthStatus> builder)
    {
        builder.HasKey(e => e.HealthStatusID);

        builder.Property(e => e.HealthStatusName).HasMaxLength(100).IsRequired();

        builder.HasData(
            new HealthStatus { HealthStatusID = 1, HealthStatusName = "سليم", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new HealthStatus { HealthStatusID = 2, HealthStatusName = "عاجز جزئياً", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new HealthStatus { HealthStatusID = 3, HealthStatusName = "عاجز كلياً", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new HealthStatus { HealthStatusID = 4, HealthStatusName = "متوفى", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new HealthStatus { HealthStatusID = 5, HealthStatusName = "شهيد", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
