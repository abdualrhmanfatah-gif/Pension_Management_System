using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class ExternalPartyConfiguration : IEntityTypeConfiguration<ExternalParty>
{
    public void Configure(EntityTypeBuilder<ExternalParty> builder)
    {
        builder.ToTable("ExternalParties");
        builder.HasKey(e => e.ExternalPartyID);

        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.PartyType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ContactInfo).HasMaxLength(1000);

        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new ExternalParty { ExternalPartyID = 1, Name = "بنك القطيبي", PartyType = "بنك", IsActive = true, CreatedAt = seedDate },
            new ExternalParty { ExternalPartyID = 2, Name = "الإدارة العامة لمكافحة المخدرات", PartyType = "إدارة أمنية", IsActive = true, CreatedAt = seedDate },
            new ExternalParty { ExternalPartyID = 3, Name = "وكيل الخدمة المدنية", PartyType = "وزارة", IsActive = true, CreatedAt = seedDate },
            new ExternalParty { ExternalPartyID = 4, Name = "الهيئة العامة للتأمينات - تعز", PartyType = "هيئة حكومية", IsActive = true, CreatedAt = seedDate },
            new ExternalParty { ExternalPartyID = 5, Name = "مدير أمن محافظة عدن", PartyType = "إدارة أمنية", IsActive = true, CreatedAt = seedDate }
        );
    }
}
