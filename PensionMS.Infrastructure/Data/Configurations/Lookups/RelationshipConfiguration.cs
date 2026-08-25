using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.HasKey(e => e.RelationshipID);

        builder.Property(e => e.RelationshipName).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Relationship { RelationshipID = 1, RelationshipName = "زوجة",  CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 2, RelationshipName = "ابن",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 3, RelationshipName = "بنت",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 4, RelationshipName = "أم",    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 5, RelationshipName = "أب",    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 6, RelationshipName = "أخت",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 7, RelationshipName = "أخ",    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Relationship { RelationshipID = 8, RelationshipName = "زوج",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
