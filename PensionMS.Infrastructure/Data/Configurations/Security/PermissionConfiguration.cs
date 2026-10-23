using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.PermissionId);

        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Category).HasMaxLength(50);

        builder.HasData(
            new Permission { PermissionId = 157, Code = "File.Transfer", Description = "نقل ملف تقاعد بين الفروع", Category = "File" },
            // ─── File (1-9, 143) ───
            new Permission { PermissionId = 143, Code = "File.View",                 Description = "عرض ملفات التقاعد",           Category = "File" },
            new Permission { PermissionId = 1,  Code = "File.Create",                Description = "إنشاء ملف تقاعد جديد",       Category = "File" },
            new Permission { PermissionId = 2,  Code = "File.Edit.BasicData",        Description = "تعديل البيانات الأساسية",     Category = "File" },
            new Permission { PermissionId = 3,  Code = "File.Edit.FinancialData",    Description = "تعديل البيانات المالية",      Category = "Financial" },
            new Permission { PermissionId = 4,  Code = "File.View.Sensitive",        Description = "رؤية الحقول الحساسة",         Category = "File" },
            new Permission { PermissionId = 5,  Code = "File.Documents.Upload",      Description = "رفع وإدارة الوثائق",          Category = "File" },
            new Permission { PermissionId = 144, Code = "File.Documents.Update",     Description = "تعديل بيانات الوثائق",        Category = "File" },
            new Permission { PermissionId = 6,  Code = "File.ChangeStatus.Return",   Description = "إعادة الملف لوجود نواقص",     Category = "File" },
            new Permission { PermissionId = 7,  Code = "File.ChangeStatus.Approve",  Description = "اعتماد الملف",                Category = "File" },
            new Permission { PermissionId = 8,  Code = "File.Override",              Description = "تجاوز قواعد النظام",          Category = "System" },
            new Permission { PermissionId = 9,  Code = "File.Delete.Soft",           Description = "حذف منطقي للملف",             Category = "File" },
            // ─── Export (10-11) ───
            new Permission { PermissionId = 10, Code = "Export.Sensitive",            Description = "تصدير بيانات حساسة",          Category = "Export" },
            new Permission { PermissionId = 11, Code = "Export.Financial",            Description = "تصدير بيانات مالية",          Category = "Export" },
            // ─── Financial (12) ───
            new Permission { PermissionId = 12, Code = "File.View.BankData",         Description = "رؤية بيانات الحساب البنكي",   Category = "Financial" },
            // ─── System (13-16) ───
            new Permission { PermissionId = 13, Code = "Audit.ViewAll",              Description = "رؤية سجلات التدقيق",          Category = "System" },
            new Permission { PermissionId = 14, Code = "System.ManageUsers",         Description = "إدارة المستخدمين والأدوار",   Category = "System" },
            new Permission { PermissionId = 15, Code = "System.Settings",            Description = "إعدادات النظام",              Category = "System" },
            new Permission { PermissionId = 16, Code = "System.Admin",               Description = "مدير النظام — صلاحية إدارية شاملة", Category = "System" },

            // ─── EntitledPerson (17-21) ───
            new Permission { PermissionId = 17, Code = "EntitledPerson.View",        Description = "عرض بيانات المستحقين",        Category = "EntitledPerson" },
            new Permission { PermissionId = 18, Code = "EntitledPerson.Create",      Description = "إضافة مستحق جديد",            Category = "EntitledPerson" },
            new Permission { PermissionId = 19, Code = "EntitledPerson.Edit",        Description = "تعديل بيانات مستحق",          Category = "EntitledPerson" },
            new Permission { PermissionId = 20, Code = "EntitledPerson.Delete",      Description = "حذف مستحق",                   Category = "EntitledPerson" },
            new Permission { PermissionId = 21, Code = "EntitledPerson.Lifecycle",   Description = "إيقاف/إعادة تفعيل استحقاق",   Category = "EntitledPerson" },
            // ─── Guardian (22-23, 49, 54, 77) ───
            new Permission { PermissionId = 22, Code = "Guardian.View",              Description = "عرض بيانات الأوصياء",          Category = "Guardian" },
            new Permission { PermissionId = 23, Code = "Guardian.Manage",            Description = "إدارة الوصايا",               Category = "Guardian" },
            new Permission { PermissionId = 49, Code = "Guardian.Add",               Description = "إضافة وصي جديد",              Category = "Guardian" },
            new Permission { PermissionId = 54, Code = "Guardian.Edit",              Description = "تعديل بيانات وصي",             Category = "Guardian" },
            new Permission { PermissionId = 77, Code = "Guardian.Delete",            Description = "حذف وصي",                     Category = "Guardian" },
            // ─── Retiree (24-26) ───
            new Permission { PermissionId = 24, Code = "Retiree.View",               Description = "عرض بيانات المتقاعدين",       Category = "Retiree" },
            new Permission { PermissionId = 25, Code = "Retiree.Edit",               Description = "تعديل بيانات المتقاعد",       Category = "Retiree" },
            new Permission { PermissionId = 26, Code = "Retiree.Delete",             Description = "حذف متقاعد",                  Category = "Retiree" },
            // ─── Agent (27-28, 50, 55, 78) ───
            new Permission { PermissionId = 27, Code = "Agent.View",                 Description = "عرض بيانات الوكلاء",          Category = "Agent" },
            new Permission { PermissionId = 28, Code = "Agent.Manage",               Description = "إدارة الوكلاء",               Category = "Agent" },
            new Permission { PermissionId = 50, Code = "Agent.Add",                  Description = "إضافة وكيل جديد",             Category = "Agent" },
            new Permission { PermissionId = 55, Code = "Agent.Edit",                 Description = "تعديل بيانات وكيل",            Category = "Agent" },
            new Permission { PermissionId = 78, Code = "Agent.Delete",               Description = "حذف وكيل",                    Category = "Agent" },
            // ─── FileNote (29-30, 60, 84, 85) ───
            new Permission { PermissionId = 29, Code = "FileNote.View",              Description = "عرض ملاحظات الملفات",          Category = "FileNote" },
            new Permission { PermissionId = 30, Code = "FileNote.Manage",            Description = "إدارة ملاحظات الملفات",        Category = "FileNote" },
            new Permission { PermissionId = 60, Code = "FileNote.Edit",              Description = "تعديل ملاحظة ملف",             Category = "FileNote" },
            new Permission { PermissionId = 84, Code = "FileNote.Add",               Description = "إضافة ملاحظة ملف",            Category = "FileNote" },
            new Permission { PermissionId = 85, Code = "FileNote.Delete",            Description = "حذف ملاحظة ملف",              Category = "FileNote" },
            // ─── DocumentExemption (31-32, 52, 57, 80) ───
            new Permission { PermissionId = 31, Code = "DocumentExemption.View",     Description = "عرض إعفاءات الوثائق",          Category = "DocumentExemption" },
            new Permission { PermissionId = 32, Code = "DocumentExemption.Manage",   Description = "إدارة إعفاءات الوثائق",        Category = "DocumentExemption" },
            new Permission { PermissionId = 52, Code = "DocumentExemption.Add",      Description = "إضافة إعفاء جديد",            Category = "DocumentExemption" },
            new Permission { PermissionId = 57, Code = "DocumentExemption.Edit",     Description = "تعديل إعفاء",                  Category = "DocumentExemption" },
            new Permission { PermissionId = 80, Code = "DocumentExemption.Delete",   Description = "حذف إعفاء",                   Category = "DocumentExemption" },
            // ─── DocumentPledge (33-34, 53, 56, 79) ───
            new Permission { PermissionId = 33, Code = "DocumentPledge.View",        Description = "عرض التعهدات",                Category = "DocumentPledge" },
            new Permission { PermissionId = 34, Code = "DocumentPledge.Manage",      Description = "إدارة التعهدات",              Category = "DocumentPledge" },
            new Permission { PermissionId = 53, Code = "DocumentPledge.Add",         Description = "إضافة تعهد جديد",             Category = "DocumentPledge" },
            new Permission { PermissionId = 56, Code = "DocumentPledge.Edit",        Description = "تعديل تعهد",                   Category = "DocumentPledge" },
            new Permission { PermissionId = 79, Code = "DocumentPledge.Delete",      Description = "حذف تعهد",                    Category = "DocumentPledge" },
            // ─── MissingDocument (35-36, 61, 86, 87) ───
            new Permission { PermissionId = 35, Code = "MissingDocument.View",       Description = "عرض الوثائق الناقصة",          Category = "MissingDocument" },
            new Permission { PermissionId = 36, Code = "MissingDocument.Manage",     Description = "إدارة الوثائق الناقصة",        Category = "MissingDocument" },
            new Permission { PermissionId = 61, Code = "MissingDocument.Edit",       Description = "تعديل وثيقة ناقصة",            Category = "MissingDocument" },
            new Permission { PermissionId = 86, Code = "MissingDocument.Add",        Description = "إضافة وثيقة ناقصة",           Category = "MissingDocument" },
            new Permission { PermissionId = 87, Code = "MissingDocument.Delete",     Description = "حذف وثيقة ناقصة",             Category = "MissingDocument" },
            // ─── PaymentBook (37-38, 51, 58, 81) ───
            new Permission { PermissionId = 37, Code = "PaymentBook.View",           Description = "عرض دفاتر الصرف",             Category = "PaymentBook" },
            new Permission { PermissionId = 38, Code = "PaymentBook.Manage",         Description = "إدارة دفاتر الصرف",           Category = "PaymentBook" },
            new Permission { PermissionId = 51, Code = "PaymentBook.Add",            Description = "إصدار دفتر صرف جديد",         Category = "PaymentBook" },
            new Permission { PermissionId = 58, Code = "PaymentBook.Edit",           Description = "تعديل دفتر صرف",              Category = "PaymentBook" },
            new Permission { PermissionId = 81, Code = "PaymentBook.Delete",         Description = "حذف دفتر صرف",                Category = "PaymentBook" },
            // ─── Ministry (39-40, 62, 90, 91) ───
            new Permission { PermissionId = 39, Code = "Ministry.View",              Description = "عرض كشوفات إحالة إلى التقاعد",     Category = "Ministry" },
            new Permission { PermissionId = 40, Code = "Ministry.Manage",            Description = "إدارة كشوفات إحالة إلى التقاعد",   Category = "Ministry" },
            new Permission { PermissionId = 62, Code = "Ministry.Edit",              Description = "تعديل كشف إحالة إلى التقاعد",         Category = "Ministry" },
            new Permission { PermissionId = 90, Code = "Ministry.Add",               Description = "إضافة كشف إحالة إلى التقاعد",        Category = "Ministry" },
            new Permission { PermissionId = 91, Code = "Ministry.Delete",            Description = "حذف كشف إحالة إلى التقاعد",          Category = "Ministry" },
            // ─── Suspension (41-42, 63, 65, 83) ───
            new Permission { PermissionId = 41, Code = "Suspension.View",            Description = "عرض إيقافات المعاش",           Category = "Suspension" },
            new Permission { PermissionId = 42, Code = "Suspension.Manage",          Description = "إدارة إيقافات المعاش",         Category = "Suspension" },
            new Permission { PermissionId = 63, Code = "Suspension.Edit",            Description = "تعديل إيقاف معاش",             Category = "Suspension" },
            new Permission { PermissionId = 65, Code = "Suspension.Add",             Description = "إضافة إيقاف معاش",             Category = "Suspension" },
            new Permission { PermissionId = 83, Code = "Suspension.Delete",          Description = "حذف إيقاف معاش",              Category = "Suspension" },
            // ─── Dashboard (43, 104) ───
            new Permission { PermissionId = 43, Code = "Dashboard.View",             Description = "عرض لوحة التحكم",             Category = "Dashboard" },
            new Permission { PermissionId = 104, Code = "HR.Dashboard.View", Description = "عرض لوحة الشؤون الإدارية", Category = "HR" },
            // ─── Settlement (44-45, 64, 88, 89) ───
            new Permission { PermissionId = 44, Code = "Settlement.View",            Description = "عرض التسويات",                Category = "Settlement" },
            new Permission { PermissionId = 45, Code = "Settlement.Manage",          Description = "إدارة التسويات",              Category = "Settlement" },
            new Permission { PermissionId = 64, Code = "Settlement.Edit",            Description = "تعديل تسوية",                  Category = "Settlement" },
            new Permission { PermissionId = 88, Code = "Settlement.Add",             Description = "إضافة تسوية",                 Category = "Settlement" },
            new Permission { PermissionId = 89, Code = "Settlement.Delete",          Description = "حذف تسوية",                   Category = "Settlement" },
            // ─── SalaryRecipient (46-48, 59, 82) ───
            new Permission { PermissionId = 46, Code = "SalaryRecipient.View",       Description = "عرض مستلمي المعاشات",           Category = "SalaryRecipient" },
            new Permission { PermissionId = 47, Code = "SalaryRecipient.Manage",     Description = "إدارة مستلمي المعاشات",         Category = "SalaryRecipient" },
            new Permission { PermissionId = 48, Code = "SalaryRecipient.Add",        Description = "إضافة مستلم معاش",             Category = "SalaryRecipient" },
            new Permission { PermissionId = 59, Code = "SalaryRecipient.Edit",       Description = "تعديل مستلم معاش",             Category = "SalaryRecipient" },
            new Permission { PermissionId = 82, Code = "SalaryRecipient.Delete",     Description = "حذف مستلم معاش",              Category = "SalaryRecipient" },

            // ─── SalaryPaymentBatch (66-70, 98, 99) ───
            new Permission { PermissionId = 66, Code = "SalaryBatch.View",           Description = "عرض كشوفات صرف المعاشات",       Category = "SalaryBatch" },
            new Permission { PermissionId = 67, Code = "SalaryBatch.Manage",         Description = "إدارة كشوفات صرف المعاشات",     Category = "SalaryBatch" },
            new Permission { PermissionId = 68, Code = "SalaryBatch.Approve",        Description = "اعتماد كشوفات صرف المعاشات",    Category = "SalaryBatch" },
            new Permission { PermissionId = 69, Code = "SalaryBatch.Export",         Description = "تصدير كشوفات صرف المعاشات",     Category = "SalaryBatch" },
            new Permission { PermissionId = 70, Code = "SalaryBatch.ImportResults",  Description = "استيراد نتائج الحوالات لكشوف المعاشات", Category = "SalaryBatch" },
            new Permission { PermissionId = 98, Code = "Payroll.ExcludeBeneficiary", Description = "استبعاد مستفيد من كشف صرف",   Category = "SalaryBatch" },
            new Permission { PermissionId = 99, Code = "Payroll.ManageDeductions",   Description = "إدارة الاستقطاعات المالية",    Category = "SalaryBatch" },

            // ─── Investment (73-76, 95-97) ───
            new Permission { PermissionId = 73, Code = "Investment.View",            Description = "عرض الاستثمارات",             Category = "Investment" },
            new Permission { PermissionId = 74, Code = "Investment.Manage",          Description = "إدارة الاستثمارات",           Category = "Investment" },
            new Permission { PermissionId = 75, Code = "Investment.Finance",         Description = "المالية الاستثمارية",          Category = "Investment" },
            new Permission { PermissionId = 76, Code = "Investment.Approve",         Description = "اعتماد الاستثمارات",           Category = "Investment" },
            new Permission { PermissionId = 95, Code = "Investment.Add",             Description = "إضافة استثمار",               Category = "Investment" },
            new Permission { PermissionId = 96, Code = "Investment.Edit",            Description = "تعديل استثمار",               Category = "Investment" },
            new Permission { PermissionId = 97, Code = "Investment.Delete",          Description = "حذف استثمار",                 Category = "Investment" },

            // ─── System Specific (100-103) ───
            new Permission { PermissionId = 100, Code = "System.Settings.Banks",     Description = "إدارة بيانات البنوك",         Category = "System" },
            new Permission { PermissionId = 101, Code = "System.Settings.DocumentTypes", Description = "إدارة أنواع الوثائق",      Category = "System" },
            new Permission { PermissionId = 102, Code = "System.Settings.JobTitles",  Description = "إدارة المسميات الوظيفية",     Category = "System" },
            new Permission { PermissionId = 103, Code = "System.Settings.InvestmentTypes", Description = "إدارة أنواع الاستثمارات", Category = "System" },

            // ─── HR (105-111) ───
            new Permission { PermissionId = 105, Code = "HR.Reference.View", Description = "عرض البيانات المرجعية للشؤون الإدارية", Category = "HR" },
            new Permission { PermissionId = 106, Code = "HR.Reference.Manage", Description = "إدارة البيانات المرجعية للشؤون الإدارية", Category = "HR" },
            new Permission { PermissionId = 107, Code = "HR.Employee.View", Description = "عرض الموظفين", Category = "HR" },
            new Permission { PermissionId = 108, Code = "HR.Employee.Create", Description = "إضافة موظف", Category = "HR" },
            new Permission { PermissionId = 109, Code = "HR.Employee.Edit", Description = "تعديل موظف", Category = "HR" },
            new Permission { PermissionId = 110, Code = "HR.Employee.Delete", Description = "حذف موظف", Category = "HR" },
            new Permission { PermissionId = 111, Code = "HR.ServiceHistory.View", Description = "عرض سجل الخدمة", Category = "HR" },
            new Permission { PermissionId = 112, Code = "File.View.History", Description = "عرض سجل العمليات وتاريخ الملف", Category = "File" },
            
            // ─── Financial Advanced & Reporting (113-116) ───
            new Permission { PermissionId = 113, Code = "Financial.Reports.View",         Description = "عرض التقارير المالية المتقدمة",   Category = "Financial" },
            new Permission { PermissionId = 114, Code = "Financial.Bank.Statement.View", Description = "عرض كشوفات الحساب البنكي",      Category = "Financial" },
            new Permission { PermissionId = 116, Code = "System.Audit.Export",            Description = "تصدير سجلات التدقيق والعمليات", Category = "System" },

            // ─── Correspondence (121-124) ───
            new Permission { PermissionId = 121, Code = "Correspondence.View",   Description = "عرض المراسلات",       Category = "Correspondence" },
            new Permission { PermissionId = 122, Code = "Correspondence.Create", Description = "إضافة قيد مراسلة",    Category = "Correspondence" },
            new Permission { PermissionId = 123, Code = "Correspondence.Edit",   Description = "تعديل قيد مراسلة",    Category = "Correspondence" },
            new Permission { PermissionId = 124, Code = "Correspondence.Delete", Description = "حذف قيد مراسلة",      Category = "Correspondence" },

            // ─── File Review (125-127) ───
            new Permission { PermissionId = 125, Code = "File.Review.View",    Description = "عرض لوحة مراجعة الملف",     Category = "File" },
            new Permission { PermissionId = 126, Code = "File.Review.Approve", Description = "اعتماد مراجعة الملف",       Category = "File" },
            new Permission { PermissionId = 127, Code = "File.Review.Reject",  Description = "رفض مراجعة الملف",          Category = "File" },

            // ─── Inventory (128-139) ───
            new Permission { PermissionId = 130, Code = "Inventory.Item.View",        Description = "عرض أصناف المخزون",         Category = "Inventory" },
            new Permission { PermissionId = 131, Code = "Inventory.Item.Create",      Description = "إضافة صنف مخزون جديد",      Category = "Inventory" },
            new Permission { PermissionId = 132, Code = "Inventory.Item.Edit",        Description = "تعديل صنف مخزون",           Category = "Inventory" },
            new Permission { PermissionId = 133, Code = "Inventory.Item.Delete",      Description = "حذف صنف مخزون",             Category = "Inventory" },
            new Permission { PermissionId = 134, Code = "Inventory.Custody.View",     Description = "عرض العهد الشخصية",         Category = "Inventory" },
            new Permission { PermissionId = 135, Code = "Inventory.Custody.Create",   Description = "تسليم عهدة جديدة",          Category = "Inventory" },
            new Permission { PermissionId = 136, Code = "Inventory.Custody.Return",   Description = "إرجاع عهدة",                Category = "Inventory" },
            new Permission { PermissionId = 137, Code = "Inventory.Transaction.View", Description = "عرض حركات المخزون",          Category = "Inventory" },
            new Permission { PermissionId = 138, Code = "Inventory.Transaction.Create", Description = "تسجيل حركة مخزون",       Category = "Inventory" },
            new Permission { PermissionId = 139, Code = "Inventory.Stocktake",        Description = "تسوية الجرد",               Category = "Inventory" },

            // ─── Accounting (140-142) ───
            new Permission { PermissionId = 140, Code = "Accounting.View",            Description = "عرض بيانات المحاسبة",        Category = "Accounting" },
            new Permission { PermissionId = 141, Code = "Accounting.Manage",          Description = "إدارة المحاسبة",              Category = "Accounting" },
            new Permission { PermissionId = 142, Code = "Accounting.Reports",         Description = "عرض التقارير المحاسبية",     Category = "Accounting" },

            // ─── Report (151-156) ───
            new Permission { PermissionId = 151, Code = "Report.Pension.View",        Description = "عرض تقارير المعاشات",        Category = "Report" },
            new Permission { PermissionId = 152, Code = "Report.Financial.View",      Description = "عرض التقارير المالية",       Category = "Report" },
            new Permission { PermissionId = 153, Code = "Report.Entitlements.View",   Description = "عرض تقارير الاستحقاقات",     Category = "Report" },
            new Permission { PermissionId = 154, Code = "Report.Documents.View",      Description = "عرض تقارير الوثائق",         Category = "Report" },
            new Permission { PermissionId = 155, Code = "Report.Export",              Description = "تصدير التقارير",             Category = "Report" },
            new Permission { PermissionId = 156, Code = "Report.Admin.View",          Description = "عرض التقارير الإدارية",      Category = "Report" });
    }
}
