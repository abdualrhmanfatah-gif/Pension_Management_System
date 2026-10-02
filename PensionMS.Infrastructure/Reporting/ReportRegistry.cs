using PensionMS.Core.Reporting;

namespace PensionMS.Infrastructure.Reporting;

public class ReportRegistry : IReportRegistry
{
    private readonly List<ReportDefinition> _reports;

    public ReportRegistry()
    {
        _reports = new List<ReportDefinition>
        {
            // ── Basic Entity Exports ──
            new ReportDefinition
            {
                Id = "basic-retirees",
                Name = "Retirees",
                NameAr = "المتقاعدون",
                Category = "Basic",
                Permission = "Report.Pension.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FullName", Label = "الاسم الكامل", Width = 70 },
                    new ReportColumn { Key = "NationalID", Label = "الهوية الوطنية", Width = 40 },
                    new ReportColumn { Key = "Governorate", Label = "المحافظة", Width = 30 },
                    new ReportColumn { Key = "Phone", Label = "الهاتف", Width = 30 },
                    new ReportColumn { Key = "SocialStatusName", Label = "الحالة الاجتماعية", Width = 30 },
                    new ReportColumn { Key = "HealthStatusName", Label = "الحالة الصحية", Width = 30 },
                    new ReportColumn { Key = "CreatedAt", Label = "تاريخ التسجيل", Type = ReportColumnType.Date, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "basic-retirement-files",
                Name = "RetirementFiles",
                NameAr = "ملفات التقاعد",
                Category = "Basic",
                Permission = "Report.Pension.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "RetireeName", Label = "اسم المتقاعد", Width = 50 },
                    new ReportColumn { Key = "RankName", Label = "الرتبة", Width = 30 },
                    new ReportColumn { Key = "BranchName", Label = "الفرع", Width = 30 },
                    new ReportColumn { Key = "StatusName", Label = "الحالة", Width = 30 },
                    new ReportColumn { Key = "FileOpenDate", Label = "تاريخ التقاعد", Type = ReportColumnType.Date, Width = 30 },
                    new ReportColumn { Key = "CreatedAt", Label = "تاريخ الإنشاء", Type = ReportColumnType.Date, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "basic-entitled-persons",
                Name = "EntitledPersons",
                NameAr = "المستحقون",
                Category = "Basic",
                Permission = "Report.Entitlements.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FullName", Label = "الاسم الكامل", Width = 50 },
                    new ReportColumn { Key = "NationalID", Label = "الهوية الوطنية", Width = 40 },
                    new ReportColumn { Key = "RelationshipName", Label = "صلة القرابة", Width = 30 },
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "SocialStatusName", Label = "الحالة الاجتماعية", Width = 30 },
                    new ReportColumn { Key = "Phone", Label = "الهاتف", Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "basic-users",
                Name = "Users",
                NameAr = "المستخدمون",
                Category = "Basic",
                Permission = "Report.Admin.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FullName", Label = "الاسم الكامل", Width = 50 },
                    new ReportColumn { Key = "Username", Label = "اسم المستخدم", Width = 40 },
                    new ReportColumn { Key = "Email", Label = "البريد الإلكتروني", Width = 50 },
                    new ReportColumn { Key = "RoleName", Label = "الدور", Width = 30 },
                    new ReportColumn { Key = "BranchName", Label = "الفرع", Width = 30 },
                    new ReportColumn { Key = "IsActive", Label = "الحالة", Type = ReportColumnType.Boolean, Width = 20 },
                    new ReportColumn { Key = "LastLogin", Label = "آخر دخول", Width = 40 }
                }
            },

            // ── Investment Reports ──
            new ReportDefinition
            {
                Id = "inv-01",
                Name = "PortfolioPerformance",
                NameAr = "أداء المحافظ",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "PortfolioName", Label = "المحفظة", Width = 50 },
                    new ReportColumn { Key = "TotalInvested", Label = "إجمالي الاستثمار", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "CurrentValue", Label = "القيمة الحالية", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "ProfitLoss", Label = "الربح/الخسارة", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Roi", Label = "العائد ROI", Type = ReportColumnType.Percent, Width = 30 },
                    new ReportColumn { Key = "ActiveProjects", Label = "المشاريع النشطة", Type = ReportColumnType.Number, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-02",
                Name = "Occupancy",
                NameAr = "نسب الإشغال",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "PropertyName", Label = "العقار", Width = 50 },
                    new ReportColumn { Key = "ProjectName", Label = "المشروع", Width = 50 },
                    new ReportColumn { Key = "TotalUnits", Label = "إجمالي الوحدات", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "OccupiedUnits", Label = "مؤجرة", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "VacantUnits", Label = "شاغرة", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "OccupancyRate", Label = "نسبة الإشغال", Type = ReportColumnType.Percent, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-03",
                Name = "RentCollection",
                NameAr = "تحصيل الإيجارات",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "TenantName", Label = "المستأجر", Width = 50 },
                    new ReportColumn { Key = "PropertyName", Label = "العقار", Width = 50 },
                    new ReportColumn { Key = "DueAmount", Label = "المبلغ المستحق", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "PaidAmount", Label = "المدفوع", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "OutstandingAmount", Label = "المتبقي", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "OverdueDays", Label = "أيام التأخير", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "PaymentStatus", Label = "الحالة", Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-04",
                Name = "ContractStatus",
                NameAr = "حالة العقود",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "TenantName", Label = "المستأجر", Width = 50 },
                    new ReportColumn { Key = "PropertyName", Label = "العقار", Width = 50 },
                    new ReportColumn { Key = "Status", Label = "حالة العقد", Width = 30 },
                    new ReportColumn { Key = "StartDate", Label = "تاريخ البداية", Type = ReportColumnType.Date, Width = 30 },
                    new ReportColumn { Key = "EndDate", Label = "تاريخ النهاية", Type = ReportColumnType.Date, Width = 30 },
                    new ReportColumn { Key = "AnnualRent", Label = "القيمة السنوية", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "DaysToExpiry", Label = "أيام للانتهاء", Type = ReportColumnType.Number, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-05",
                Name = "RiskExposure",
                NameAr = "المخاطر الاستثمارية",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "ProjectName", Label = "المشروع", Width = 50 },
                    new ReportColumn { Key = "RiskType", Label = "نوع الخطر", Width = 30 },
                    new ReportColumn { Key = "Severity", Label = "الخطورة", Width = 25 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 25 },
                    new ReportColumn { Key = "MitigationPlan", Label = "خطة التخفيف", Width = 50 },
                    new ReportColumn { Key = "IdentifiedDate", Label = "تاريخ التقييم", Type = ReportColumnType.Date, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-06",
                Name = "MaintenanceSummary",
                NameAr = "طلبات الصيانة",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "PropertyName", Label = "العقار", Width = 50 },
                    new ReportColumn { Key = "Description", Label = "الوصف", Width = 60 },
                    new ReportColumn { Key = "Priority", Label = "الأولوية", Width = 25 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 25 },
                    new ReportColumn { Key = "EstimatedCost", Label = "التكلفة التقديرية", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "ActualCost", Label = "التكلفة الفعلية", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "ResponseDays", Label = "أيام الاستجابة", Type = ReportColumnType.Number, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "inv-07",
                Name = "BudgetVsActual",
                NameAr = "الميزانية والمصروفات",
                Category = "Investment",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "ProjectName", Label = "المشروع", Width = 50 },
                    new ReportColumn { Key = "FiscalYear", Label = "السنة المالية", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "PlannedAmount", Label = "المبلغ المخطط", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "ActualSpent", Label = "المنصرف الفعلي", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Variance", Label = "الانحراف", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "VariancePct", Label = "نسبة الانحراف", Type = ReportColumnType.Percent, Width = 30 }
                }
            },

            // ── Retirement Reports ──
            new ReportDefinition
            {
                Id = "ret-01",
                Name = "FileStatus",
                NameAr = "حالة الملفاتات",
                Category = "Retirement",
                Permission = "Report.Pension.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "RetireeName", Label = "المتقاعد", Width = 50 },
                    new ReportColumn { Key = "RankName", Label = "الرتبة", Width = 30 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 30 },
                    new ReportColumn { Key = "DaysPending", Label = "أيام الانتظار", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "MissingDocsCount", Label = "مستندات ناقصة", Type = ReportColumnType.Number, Width = 25 }
                }
            },
            new ReportDefinition
            {
                Id = "ret-02",
                Name = "ActiveRetirees",
                NameAr = "المتقاعدين النشطين",
                Category = "Retirement",
                Permission = "Report.Pension.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FullName", Label = "الاسم", Width = 50 },
                    new ReportColumn { Key = "NationalID", Label = "الهوية", Width = 40 },
                    new ReportColumn { Key = "RankName", Label = "الرتبة", Width = 30 },
                    new ReportColumn { Key = "Category", Label = "الفئة", Width = 30 },
                    new ReportColumn { Key = "MonthlyPension", Label = "المعاش الشهري", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Governorate", Label = "المحافظة", Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "ret-03",
                Name = "Suspensions",
                NameAr = "سجل الإيقافات",
                Category = "Retirement",
                Permission = "Report.Pension.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "RetireeName", Label = "المتقاعد", Width = 50 },
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "Reason", Label = "السبب", Width = 40 },
                    new ReportColumn { Key = "SuspendedDate", Label = "تاريخ الإيقاف", Type = ReportColumnType.Date, Width = 30 },
                    new ReportColumn { Key = "DaysSuspended", Label = "عدد الأيام", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "IsActive", Label = "نشط", Type = ReportColumnType.Boolean, Width = 20 }
                }
            },

            // ── Financial Reports ──
            new ReportDefinition
            {
                Id = "fin-01",
                Name = "SalaryBatches",
                NameAr = "مسيرات المعاشات",
                Category = "Financial",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "Period", Label = "الفترة", Width = 40 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 30 },
                    new ReportColumn { Key = "TotalAmount", Label = "إجمالي المبلغ", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "RecipientCount", Label = "عدد المستفيدين", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "CreatedAt", Label = "تاريخ الإنشاء", Type = ReportColumnType.Date, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "fin-02",
                Name = "Settlements",
                NameAr = "التسويات المالية",
                Category = "Financial",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "DecisionNumber", Label = "رقم القرار", Width = 40 },
                    new ReportColumn { Key = "RetireeName", Label = "المتقاعد", Width = 50 },
                    new ReportColumn { Key = "Amount", Label = "المبلغ", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 30 },
                    new ReportColumn { Key = "CreatedAt", Label = "تاريخ الإنشاء", Type = ReportColumnType.Date, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "fin-03",
                Name = "FiscalPeriod",
                NameAr = "الميزانية الربعية",
                Category = "Financial",
                Permission = "Report.Financial.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "ChapterName", Label = "الباب", Width = 40 },
                    new ReportColumn { Key = "SectionName", Label = "البند", Width = 40 },
                    new ReportColumn { Key = "Quarter", Label = "الربع", Width = 20 },
                    new ReportColumn { Key = "Allocated", Label = "المخصص", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Spent", Label = "المنصرف", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "Remaining", Label = "المتبقي", Type = ReportColumnType.Currency, Width = 40 },
                    new ReportColumn { Key = "SpentPct", Label = "نسبة الصرف", Type = ReportColumnType.Percent, Width = 25 }
                }
            },

            // ── Documents Reports ──
            new ReportDefinition
            {
                Id = "doc-01",
                Name = "MissingDocs",
                NameAr = "المستندات الناقصة",
                Category = "Documents",
                Permission = "Report.Documents.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "RetireeName", Label = "اسم المتقاعد", Width = 50 },
                    new ReportColumn { Key = "MilitaryNumber", Label = "الرقم العسكري", Width = 30 },
                    new ReportColumn { Key = "MissingCount", Label = "عدد النواقص", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "MissingDocsList", Label = "المستندات الناقصة", Width = 80 },
                    new ReportColumn { Key = "MaxDaysMissing", Label = "أقصى أيام التأخير", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "Phone", Label = "رقم الهاتف", Width = 30 },
                    new ReportColumn { Key = "AgentPhone", Label = "رقم هاتف الوكيل", Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "doc-01b",
                Name = "MissingDocsByPerson",
                NameAr = "المستندات الناقصة حسب الشخص",
                Category = "Documents",
                Permission = "Report.Documents.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "RetireeName", Label = "اسم المتقاعد", Width = 50 },
                    new ReportColumn { Key = "MilitaryNumber", Label = "الرقم العسكري", Width = 30 },
                    new ReportColumn { Key = "PersonType", Label = "نوع الشخص", Width = 25 },
                    new ReportColumn { Key = "PersonName", Label = "اسم الشخص", Width = 50 },
                    new ReportColumn { Key = "Relationship", Label = "صلة القرابة", Width = 25 },
                    new ReportColumn { Key = "PersonPhone", Label = "رقم الهاتف", Width = 30 },
                    new ReportColumn { Key = "MissingCount", Label = "عدد النواقص", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "MissingDocsList", Label = "المستندات الناقصة", Width = 80 },
                    new ReportColumn { Key = "MaxDaysMissing", Label = "أقصى أيام التأخير", Type = ReportColumnType.Number, Width = 30 }
                }
            },
            new ReportDefinition
            {
                Id = "doc-02",
                Name = "ExpiredDocs",
                NameAr = "المستندات المنتهية",
                Category = "Documents",
                Permission = "Report.Documents.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "DocTypeName", Label = "نوع المستند", Width = 40 },
                    new ReportColumn { Key = "OwnerName", Label = "صاحب المستند", Width = 50 },
                    new ReportColumn { Key = "OwnerType", Label = "النوع", Width = 30 },
                    new ReportColumn { Key = "ExpiryDate", Label = "تاريخ الانتهاء", Type = ReportColumnType.Date, Width = 30 },
                    new ReportColumn { Key = "DaysExpired", Label = "أيام منذ الانتهاء", Type = ReportColumnType.Number, Width = 30 },
                    new ReportColumn { Key = "IsVerified", Label = "موثق", Type = ReportColumnType.Boolean, Width = 20 }
                }
            },

            // ── Dependents ──
            new ReportDefinition
            {
                Id = "dep-01",
                Name = "Beneficiaries",
                NameAr = "المستفيدين والورثة",
                Category = "Dependents",
                Permission = "Report.Entitlements.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 30 },
                    new ReportColumn { Key = "RetireeName", Label = "اسم المتقاعد", Width = 50 },
                    new ReportColumn { Key = "MilitaryNumber", Label = "الرقم العسكري", Width = 30 },
                    new ReportColumn { Key = "FullName", Label = "اسم المستفيد", Width = 50 },
                    new ReportColumn { Key = "NationalID", Label = "الهوية", Width = 40 },
                    new ReportColumn { Key = "Relationship", Label = "صلة القرابة", Width = 30 },
                    new ReportColumn { Key = "PersonType", Label = "نوع الشخص", Width = 25 },
                    new ReportColumn { Key = "GuardianName", Label = "الوصي", Width = 40 },
                    new ReportColumn { Key = "PersonPhone", Label = "رقم الهاتف", Width = 30 },
                    new ReportColumn { Key = "MonthlyAmount", Label = "المبلغ الشهري", Type = ReportColumnType.Currency, Width = 35 }
                }
            },

            // ── Kashf (كشوفات الإحالة) ──
            new ReportDefinition
            {
                Id = "kashf-01",
                Name = "KashfReport",
                NameAr = "كشوفات الإحالة",
                Category = "Compliance",
                Permission = "Ministry.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "MilitaryNumber", Label = "الرقم العسكري", Width = 30 },
                    new ReportColumn { Key = "FullName", Label = "الاسم", Width = 50 },
                    new ReportColumn { Key = "RankName", Label = "الرتبة", Width = 25 },
                    new ReportColumn { Key = "UnitName", Label = "الوحدة", Width = 35 },
                    new ReportColumn { Key = "CategoryName", Label = "الفئة", Width = 25 },
                    new ReportColumn { Key = "Gender", Label = "الجنس", Width = 15 },
                    new ReportColumn { Key = "Salary", Label = "المعاش", Type = ReportColumnType.Currency, Width = 25 },
                    new ReportColumn { Key = "RetirementDate", Label = "تاريخ الإحالة", Type = ReportColumnType.Date, Width = 25 },
                    new ReportColumn { Key = "Status", Label = "الحالة", Width = 25 },
                    new ReportColumn { Key = "FileNumber", Label = "رقم الملف", Width = 25 }
                }
            },

            // ── Compliance ──
            new ReportDefinition
            {
                Id = "com-02",
                Name = "PendingApprovals",
                NameAr = "الموافقات المعلقة",
                Category = "Compliance",
                Permission = "Report.Admin.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "RequestType", Label = "نوع الطلب", Width = 40 },
                    new ReportColumn { Key = "ProjectName", Label = "الكيان/المشروع", Width = 50 },
                    new ReportColumn { Key = "RequestedBy", Label = "مقدم الطلب", Width = 40 },
                    new ReportColumn { Key = "DaysPending", Label = "أيام الانتظار", Type = ReportColumnType.Number, Width = 25 },
                    new ReportColumn { Key = "CreatedAt", Label = "تاريخ الإنشاء", Type = ReportColumnType.Date, Width = 30 }
                }
            },

            // ── Admin ──
            new ReportDefinition
            {
                Id = "adm-01",
                Name = "AuditLog",
                NameAr = "سجل النظام",
                Category = "Admin",
                Permission = "Report.Admin.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "UserName", Label = "المستخدم", Width = 40 },
                    new ReportColumn { Key = "Action", Label = "الإجراء", Width = 30 },
                    new ReportColumn { Key = "TableName", Label = "الجدول", Width = 40 },
                    new ReportColumn { Key = "RecordID", Label = "رقم السجل", Width = 25 },
                    new ReportColumn { Key = "Timestamp", Label = "الوقت", Type = ReportColumnType.Date, Width = 40 }
                }
            },
            new ReportDefinition
            {
                Id = "adm-02",
                Name = "UserActivity",
                NameAr = "نشاط المستخدمين",
                Category = "Admin",
                Permission = "Report.Admin.View",
                Columns = new[]
                {
                    new ReportColumn { Key = "UserName", Label = "اسم المستخدم", Width = 40 },
                    new ReportColumn { Key = "FullName", Label = "الاسم", Width = 50 },
                    new ReportColumn { Key = "RoleName", Label = "الدور", Width = 30 },
                    new ReportColumn { Key = "BranchName", Label = "الفرع", Width = 30 },
                    new ReportColumn { Key = "LastLogin", Label = "آخر دخول", Type = ReportColumnType.Date, Width = 40 },
                    new ReportColumn { Key = "IsActive", Label = "نشط", Type = ReportColumnType.Boolean, Width = 20 }
                }
            }
        };
    }

    public IReadOnlyList<ReportDefinition> GetAll() => _reports.AsReadOnly();

    public ReportDefinition? GetById(string id) =>
        _reports.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ReportDefinition> GetByCategory(string category) =>
        _reports.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
}
