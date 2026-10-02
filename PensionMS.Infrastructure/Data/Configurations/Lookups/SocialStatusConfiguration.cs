using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class SocialStatusConfiguration : IEntityTypeConfiguration<SocialStatus>
{
    public void Configure(EntityTypeBuilder<SocialStatus> builder)
    {
        builder.HasKey(e => e.SocialStatusID);

        builder.Property(e => e.StatusName).HasMaxLength(100).IsRequired();

        builder.HasData(
            new SocialStatus { SocialStatusID = 1, StatusName = "أعزب", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SocialStatus { SocialStatusID = 2, StatusName = "متزوج", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SocialStatus { SocialStatusID = 3, StatusName = "أرمل", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SocialStatus { SocialStatusID = 4, StatusName = "مطلق", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
