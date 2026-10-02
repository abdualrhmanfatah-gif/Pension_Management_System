using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Data.Seeders
{
    public static class HrDocumentTypeSeeder
    {
        public static async Task SeedAsync(PensionDbContext context)
        {
            try
            {
                // 1. Employee Document Types (OwnerType = 40)
                await SeedEmployeeDocumentTypes(context);

                // 2. Correspondence Document Types (OwnerType = 41)
                await SeedCorrespondenceDocumentTypes(context);

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log error if needed, but don't break the application startup
            }
        }

        private static async Task SeedEmployeeDocumentTypes(PensionDbContext context)
        {
            var types = new[]
            {
                new { Name = "قرار التعيين", En = "Appointment Decision", Code = "HR_EMP_001" },
                new { Name = "البطاقة الشخصية / الرقم الوطني", En = "National ID", Code = "HR_EMP_002" },
                new { Name = "المؤهل الدراسي", En = "Academic Qualification", Code = "HR_EMP_003" },
                new { Name = "شهادة الميلاد", En = "Birth Certificate", Code = "HR_EMP_004" },
                new { Name = "الرقم العسكري", En = "Military Number Proof", Code = "HR_EMP_005" },
                new { Name = "تقرير الفحص الطبي", En = "Medical Examination Report", Code = "HR_EMP_006" },
                new { Name = "مباشرة عمل", En = "Work Commencement", Code = "HR_EMP_007" },
                new { Name = "بيان حالة وظيفية", En = "Job Status Statement", Code = "HR_EMP_008" },
                new { Name = "قرار ترقية", En = "Promotion Decision", Code = "HR_EMP_009" },
                new { Name = "إجازة معتمدة", En = "Approved Leave", Code = "HR_EMP_010" }
            };

            foreach (var t in types)
            {
                if (!await context.DocumentTypes.AnyAsync(dt => dt.SystemCode == t.Code))
                {
                    context.DocumentTypes.Add(new DocumentType
                    {
                        TypeName = t.Name,
                        TypeNameEn = t.En,
                        SystemCode = t.Code,
                        OwnerType = DocumentOwnerType.Employee,
                        IsRequired = false,
                        HasExpiry = false,
                        SortOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private static async Task SeedCorrespondenceDocumentTypes(PensionDbContext context)
        {
            var types = new[]
            {
                new { Name = "أصل المراسلة الواردة", En = "Original Incoming Correspondence", Code = "HR_COR_001" },
                new { Name = "مرفق المراسلة", En = "Correspondence Attachment", Code = "HR_COR_002" },
                new { Name = "صورة طبق الأصل", En = "Certified Copy", Code = "HR_COR_003" },
                new { Name = "رد رسمي", En = "Official Reply", Code = "HR_COR_004" }
            };

            foreach (var t in types)
            {
                if (!await context.DocumentTypes.AnyAsync(dt => dt.SystemCode == t.Code))
                {
                    context.DocumentTypes.Add(new DocumentType
                    {
                        TypeName = t.Name,
                        TypeNameEn = t.En,
                        SystemCode = t.Code,
                        OwnerType = DocumentOwnerType.Correspondence,
                        IsRequired = false,
                        HasExpiry = false,
                        SortOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
