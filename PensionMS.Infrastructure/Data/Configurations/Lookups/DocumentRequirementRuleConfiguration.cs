using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class DocumentRequirementRuleConfiguration : IEntityTypeConfiguration<DocumentRequirementRule>
{
    public void Configure(EntityTypeBuilder<DocumentRequirementRule> builder)
    {
        builder.HasKey(e => e.RuleID);

        builder.Property(e => e.TargetEntity).HasMaxLength(50);
        builder.Property(e => e.ConditionField).HasMaxLength(100);
        builder.Property(e => e.ConditionValue).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasOne(e => e.DocumentType)
              .WithMany()
              .HasForeignKey(e => e.DocumentTypeID);

        builder.HasData(
            new DocumentRequirementRule { RuleID = 1, DocumentTypeID = 1, IsGlobal = true, Description = "الهوية الوطنية (عالمي)" },
            new DocumentRequirementRule { RuleID = 2, DocumentTypeID = 8, IsGlobal = true, Description = "أوليات الملف (عالمي)" },
            new DocumentRequirementRule { RuleID = 3, DocumentTypeID = 2, IsGlobal = false, TargetEntity = "Retiree", ConditionField = "IsAlive", ConditionValue = "False", Description = "شهادة الوفاة للمتوفين" },
            new DocumentRequirementRule { RuleID = 4, DocumentTypeID = 3, IsGlobal = false, TargetEntity = "Agent", ConditionField = "Exists", ConditionValue = "True", Description = "وثيقة الوكالة عند وجود وكيل" }
        );
    }
}
