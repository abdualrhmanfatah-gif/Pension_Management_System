using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.EmployeeID);

        builder.Property(e => e.EmployeeNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.NationalID).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.BirthPlace).HasMaxLength(200);
        builder.Property(e => e.MaritalStatus).HasMaxLength(50);
        builder.Property(e => e.MilitaryNumber).HasMaxLength(50);
        builder.Property(e => e.RankID).IsRequired(false);
        builder.HasOne(e => e.Rank)
            .WithMany()
            .HasForeignKey(e => e.RankID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.MilitaryServiceDate).IsRequired(false);
        builder.Property(e => e.JobPositions).HasMaxLength(2000);
        builder.Property(e => e.AcademicQualifications).HasMaxLength(2000);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.ProfilePhotoPath).HasMaxLength(500);
        builder.Property(e => e.BloodGroup).HasMaxLength(10);
        builder.Property(e => e.NationalIDIssuePlace).HasMaxLength(200);
        builder.Property(e => e.EmergencyContactName).HasMaxLength(200);
        builder.Property(e => e.EmergencyContactPhone).HasMaxLength(20);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Section)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.SectionID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.JobTitle)
            .WithMany(j => j.Employees)
            .HasForeignKey(e => e.JobTitleID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EmploymentStatus)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.EmploymentStatusID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EmployeeNumber).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.NationalID).IsUnique().HasFilter("[NationalID] <> '' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(e => new { e.DepartmentID, e.SectionID });
        
        builder.Property(e => e.RowVersion).IsRowVersion();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            // 1 - جمال فضل عبدالكريم القطيبي — المدير العام (عميد ركن)
            new Employee
            {
                EmployeeID = 1, EmployeeNumber = "EMP-001", FullName = "جمال فضل عبدالكريم القطيبي",
                NationalID = "", MilitaryNumber = "2361", RankID = 7,
                MilitaryServiceDate = new DateTime(1984, 8, 4, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 1, JobPositions = "المدير العام",
                DepartmentID = 6, EmploymentStatusID = 1,
                AcademicQualifications = "جامعي",
                BirthDate = new DateOnly(1966, 12, 31),
                Address = "المنصورة", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(1984, 8, 4, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 2 - شامخ عبدالرحمن أحمد فتاح — نائب المدير العام المدير المالي (عقيد ركن)
            new Employee
            {
                EmployeeID = 2, EmployeeNumber = "EMP-002", FullName = "شامخ عبدالرحمن أحمد فتاح",
                NationalID = "", MilitaryNumber = "24244", RankID = 6,
                MilitaryServiceDate = new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 2, JobPositions = "نائب المدير العام المدير المالي",
                DepartmentID = 6, EmploymentStatusID = 1,
                Phone = "777618680", AcademicQualifications = "دكتوراه إدارة أعمال",
                BirthDate = new DateOnly(1990, 1, 28),
                BirthPlace = "بيت الفقيه/الحديدة", Address = "المنصورة", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 3 - نائف أحمد محمد الشاجري — مساعد المدير العام (عقيد)
            new Employee
            {
                EmployeeID = 3, EmployeeNumber = "EMP-003", FullName = "نائف أحمد محمد الشاجري",
                NationalID = "", MilitaryNumber = "18789", RankID = 6,
                MilitaryServiceDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 6, JobPositions = "مساعد المدير العام",
                DepartmentID = 6, EmploymentStatusID = 1,
                AcademicQualifications = "بكالوريوس قانون",
                BirthDate = new DateOnly(1986, 5, 20),
                BirthPlace = "عدن", Address = "خورمكسر/العريش", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 4 - صالح علي مقبل حيدرة — مستشار المدير العام (عقيد)
            new Employee
            {
                EmployeeID = 4, EmployeeNumber = "EMP-004", FullName = "صالح علي مقبل حيدرة",
                NationalID = "", RankID = 6,
                JobTitleID = 6, JobPositions = "مستشار المدير العام",
                DepartmentID = 6, EmploymentStatusID = 1,
                Phone = "777355188", AcademicQualifications = "بكالوريوس",
                BirthPlace = "لحج", Address = "لحج", MaritalStatus = "متزوج", Gender = 1,
                HireDate = seedDate,
                CreatedAt = seedDate
            },
            // 5 - منصور عبدالله اليماني — مدير إدارة المعاشات (عقيد)
            new Employee
            {
                EmployeeID = 5, EmployeeNumber = "EMP-005", FullName = "منصور عبدالله اليماني",
                NationalID = "", MilitaryNumber = "7443", RankID = 6,
                JobTitleID = 3, JobPositions = "مدير إدارة المعاشات",
                DepartmentID = 1, EmploymentStatusID = 1,
                Phone = "770601789", Gender = 1,
                HireDate = seedDate,
                CreatedAt = seedDate
            },
            // 6 - كفاح عبدالرحمن أحمد فتاح — مدير التخطيط (رائد)
            new Employee
            {
                EmployeeID = 6, EmployeeNumber = "EMP-006", FullName = "كفاح عبدالرحمن أحمد فتاح",
                NationalID = "", MilitaryNumber = "33068", RankID = 4,
                MilitaryServiceDate = new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 6, JobPositions = "مدير التخطيط",
                DepartmentID = 6, EmploymentStatusID = 1,
                Phone = "771985009", AcademicQualifications = "ماجستير نظم معلومات",
                BirthDate = new DateOnly(1991, 7, 30),
                BirthPlace = "بيت الفقيه/الحديدة", Address = "المنصورة", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 7 - خالد صالح بن صالح — نائب المدير المالي رئيس الحاسب الآلي والحسابات (نقيب)
            new Employee
            {
                EmployeeID = 7, EmployeeNumber = "EMP-007", FullName = "خالد صالح بن صالح",
                NationalID = "291268142893", MilitaryNumber = "27940", RankID = 3,
                MilitaryServiceDate = new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 5, JobPositions = "نائب المدير المالي رئيس الحاسب الآلي والحسابات",
                DepartmentID = 4, SectionID = 14, EmploymentStatusID = 1,
                Phone = "778306198", AcademicQualifications = "دبلوم + 7 دورات كمبيوتر",
                BirthDate = new DateOnly(1973, 3, 6),
                BirthPlace = "عدن/صيرة", Address = "مديرية الشيخ عثمان/الممدارة", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 8 - آمنة علي عياش سالم — رئيس قسم الأرشيف (مساعد/1)
            new Employee
            {
                EmployeeID = 8, EmployeeNumber = "EMP-008", FullName = "آمنة علي عياش سالم",
                NationalID = "108121943248", MilitaryNumber = "185650", RankID = 17,
                MilitaryServiceDate = new DateTime(2008, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 5, JobPositions = "رئيس قسم الأرشيف",
                DepartmentID = 1, SectionID = 6, EmploymentStatusID = 1,
                Phone = "783421792", AcademicQualifications = "ثانوية عامة",
                BirthDate = new DateOnly(1987, 8, 13),
                BirthPlace = "عدن/دار سعد", Address = "عدن/دار سعد/الميدان", MaritalStatus = "متزوجة", Gender = 0,
                HireDate = new DateTime(2008, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 9 - رنا علي هربي يوسف — مختص البحث الاجتماعي (عريف)
            new Employee
            {
                EmployeeID = 9, EmployeeNumber = "EMP-009", FullName = "رنا علي هربي يوسف",
                NationalID = "495335740730", MilitaryNumber = "305561", RankID = 12,
                MilitaryServiceDate = new DateTime(2015, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 6, JobPositions = "مختص البحث الاجتماعي",
                DepartmentID = 1, SectionID = 1, EmploymentStatusID = 1,
                Phone = "772373136", AcademicQualifications = "بكالوريوس قسم حاسوب",
                BirthDate = new DateOnly(1994, 5, 18),
                BirthPlace = "عدن/صيرة", Address = "خورمكسر/العريش", MaritalStatus = "عازبة", Gender = 0,
                HireDate = new DateTime(2015, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 10 - أحمد الخضر أحمد — مختص الأرشيف (جندي)
            new Employee
            {
                EmployeeID = 10, EmployeeNumber = "EMP-010", FullName = "أحمد الخضر أحمد",
                NationalID = "", RankID = 11,
                JobTitleID = 6, JobPositions = "مختص الأرشيف",
                DepartmentID = 1, SectionID = 6, EmploymentStatusID = 1,
                Phone = "776859111", Gender = 1,
                HireDate = seedDate,
                CreatedAt = seedDate
            },
            // 11 - ماهر هبه علي هبه — رئيس قسم المخازن (جندي)
            new Employee
            {
                EmployeeID = 11, EmployeeNumber = "EMP-011", FullName = "ماهر هبه علي هبه",
                NationalID = "", MilitaryNumber = "374013", RankID = 11,
                MilitaryServiceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 5, JobPositions = "رئيس قسم المخازن",
                DepartmentID = 4, SectionID = 18, EmploymentStatusID = 1,
                Phone = "784881417", AcademicQualifications = "ثانوية",
                BirthDate = new DateOnly(1988, 1, 2),
                BirthPlace = "الحديدة", Address = "الثقنية", MaritalStatus = "عازب", Gender = 1,
                HireDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            // 12 - عمرو خالد صالح — رئيس قسم المراجعة (مدني)
            new Employee
            {
                EmployeeID = 12, EmployeeNumber = "EMP-012", FullName = "عمرو خالد صالح",
                NationalID = "",
                JobTitleID = 5, JobPositions = "رئيس قسم المراجعة",
                DepartmentID = 4, SectionID = 17, EmploymentStatusID = 1,
                AcademicQualifications = "بكالوريوس هندسة مدنية",
                BirthDate = new DateOnly(2003, 7, 24),
                BirthPlace = "عدن", Address = "عدن/الشيخ عثمان/الممدارة", MaritalStatus = "عازب", Gender = 1,
                HireDate = seedDate,
                CreatedAt = seedDate
            },
            // 13 - محمد عبده علي — مختص النظافة (جندي)
            new Employee
            {
                EmployeeID = 13, EmployeeNumber = "EMP-013", FullName = "محمد عبده علي",
                NationalID = "", RankID = 11,
                MilitaryServiceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                JobTitleID = 6, JobPositions = "مختص النظافة",
                DepartmentID = 2, EmploymentStatusID = 1,
                Address = "الصولبان غازي علوان", MaritalStatus = "متزوج", Gender = 1,
                HireDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            }
        );
    }
}
