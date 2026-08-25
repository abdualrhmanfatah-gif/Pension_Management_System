using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(e => e.SettingID);

        builder.Property(e => e.SettingKey).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SettingValue).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasData(
            new SystemSetting { SettingID = 1, SettingKey = "OrganizationName", SettingValue = "هيئة التقاعد الوطنية", Category = "General", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemSetting { SettingID = 2, SettingKey = "LegalRetirementAge", SettingValue = "63", Category = "Pension", Description = "السن القانوني للتقاعد", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemSetting { SettingID = 3, SettingKey = "MinimumServiceYears", SettingValue = "15", Category = "Pension", Description = "الحد الأدنى لسنوات الخدمة لاستحقاق المعاش", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemSetting { SettingID = 4, SettingKey = "Currency", SettingValue = "د.ع", Category = "Finance", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
