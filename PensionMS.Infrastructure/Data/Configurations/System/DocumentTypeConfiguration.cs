using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.System;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.HasKey(e => e.DocumentTypeID);

        builder.Property(e => e.TypeName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SystemCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TypeNameEn).HasMaxLength(200);
        builder.Property(e => e.AllowedExtensions).HasMaxLength(500);

        builder.HasData(
            new DocumentType { DocumentTypeID = 1, TypeName = "الهوية الوطنية",   TypeNameEn = "National ID",      IsRequired = true,  HasExpiry = true,  SortOrder = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 2, TypeName = "شهادة الوفاة",     TypeNameEn = "Death Certificate",        IsRequired = false, HasExpiry = false, SortOrder = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 3, TypeName = "وثيقة الوكالة",    TypeNameEn = "Agency Document",          IsRequired = false, HasExpiry = true,  SortOrder = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 4, TypeName = "وثيقة الوصاية",    TypeNameEn = "Guardianship Document",    IsRequired = false, HasExpiry = true,  SortOrder = 4, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 5, TypeName = "اثبات عزوبية",     TypeNameEn = "Single Status Certificate",IsRequired = false, HasExpiry = false, SortOrder = 5, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 6, TypeName = "عقد زواج",         TypeNameEn = "Marriage Contract",        IsRequired = false, HasExpiry = false, SortOrder = 6, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 7, TypeName = "حصر ورثة",         TypeNameEn = "Inheritance Certificate",  IsRequired = false, HasExpiry = false, SortOrder = 7, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 8,  TypeName = "أوليات الملف",    TypeNameEn = "File Originals",           IsRequired = false, HasExpiry = false, SortOrder = 8, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 9,  TypeName = "وثيقة تعهد",      TypeNameEn = "Pledge Document",          IsRequired = false, HasExpiry = false, SortOrder = 9, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 10, TypeName = "توجيه بإعفاء من النواقص", TypeNameEn = "Missing Documents Exemption Directive", IsRequired = false, HasExpiry = false, SortOrder = 10, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 11, TypeName = "أمر إيقاف المعاش", TypeNameEn = "Salary Suspension Order",  IsRequired = false, HasExpiry = false, SortOrder = 11, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 12, TypeName = "مرفق ملاحظة",     TypeNameEn = "Note Attachment",          IsRequired = false, HasExpiry = false, SortOrder = 12, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 13, TypeName = "صورة دفتر الصرف", TypeNameEn = "Payment Book Image",       IsRequired = false, HasExpiry = false, SortOrder = 13, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 14, TypeName = "وثيقة طلاق",       TypeNameEn = "Divorce Certificate",      IsRequired = false, HasExpiry = false, SortOrder = 14, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 15, TypeName = "إثبات الخدمة المدنية", TypeNameEn = "Civil Service Proof",  IsRequired = false, HasExpiry = false, SortOrder = 15, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 16, TypeName = "تقرير طبي",         TypeNameEn = "Medical Report",           IsRequired = false, HasExpiry = false, SortOrder = 16, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 17, TypeName = "حكم إعالة",          TypeNameEn = "Maintenance Judgment",     IsRequired = false, HasExpiry = false, SortOrder = 17, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DocumentType { DocumentTypeID = 99, TypeName = "الهوية الوطنية (ظهر)", TypeNameEn = "National ID (Back)",     IsRequired = false, HasExpiry = true,  SortOrder = 99, IsActive = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
