using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.System;

public class SystemTableConfiguration : IEntityTypeConfiguration<SystemTable>
{
    public void Configure(EntityTypeBuilder<SystemTable> builder)
    {
        builder.HasKey(e => e.TableID);

        builder.Property(e => e.TableNameEn).HasMaxLength(100).IsRequired();
        builder.Property(e => e.TableNameAr).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasData(
            new SystemTable { TableID = 1,  TableNameEn = "Retirees",               TableNameAr = "المتقاعدون",            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 2,  TableNameEn = "RetirementFiles",         TableNameAr = "ملفات التقاعد",         CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 3,  TableNameEn = "EntitledPersons",         TableNameAr = "المستحقون",             CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 4,  TableNameEn = "Agents",                  TableNameAr = "الوكلاء",               CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 5,  TableNameEn = "Guardians",               TableNameAr = "الأوصياء",              CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 6,  TableNameEn = "Documents",               TableNameAr = "الوثائق",               CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 7,  TableNameEn = "DocumentPledges",         TableNameAr = "التعهدات",              CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 8,  TableNameEn = "MissingDocuments",        TableNameAr = "الوثائق الناقصة",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 9,  TableNameEn = "DocumentExemptions",      TableNameAr = "إعفاءات الوثائق",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 10, TableNameEn = "FileNotes",               TableNameAr = "ملاحظات الملفات",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 11, TableNameEn = "PensionSuspensions",      TableNameAr = "إيقاف المعاش",          CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 12, TableNameEn = "SalaryRecipients",        TableNameAr = "مستلمو المعاشات",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 13, TableNameEn = "FinancialEntitlements",    TableNameAr = "الاستحقاقات المالية",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 14, TableNameEn = "SalaryPaymentBatches",    TableNameAr = "كشوفات المعاشات",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 15, TableNameEn = "SalaryPaymentItems",      TableNameAr = "تفاصيل المعاشات",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 16, TableNameEn = "MinistryApprovalBatches", TableNameAr = "كشوفات اعتماد القوة",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 17, TableNameEn = "MinistryApprovalItems",   TableNameAr = "تفاصيل اعتماد القوة",   CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 18, TableNameEn = "SettlementDecisions",     TableNameAr = "قرارات التسوية",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 19, TableNameEn = "SettlementsBatches",      TableNameAr = "كشوفات التسويات",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 20, TableNameEn = "SettlementItems",         TableNameAr = "تفاصيل التسويات",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 21, TableNameEn = "Users",                   TableNameAr = "المستخدمون",            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 22, TableNameEn = "LoginHistories",          TableNameAr = "سجل الدخول",            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 23, TableNameEn = "Roles",                   TableNameAr = "الأدوار",               CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 24, TableNameEn = "TablePermissions",        TableNameAr = "صلاحيات الجداول",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 25, TableNameEn = "Notifications",           TableNameAr = "الإشعارات",             CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 26, TableNameEn = "AuditLogs",               TableNameAr = "سجل العمليات",          CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 27, TableNameEn = "SyncQueues",              TableNameAr = "قائمة المزامنة",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 28, TableNameEn = "DeviceRegistrations",     TableNameAr = "الأجهزة المسجلة",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 29, TableNameEn = "Ranks",                   TableNameAr = "الرتب العسكرية",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 30, TableNameEn = "Banks",                   TableNameAr = "البنوك",                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 31, TableNameEn = "Relationships",           TableNameAr = "صلات القرابة",          CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 32, TableNameEn = "DocumentTypes",           TableNameAr = "أنواع الوثائق",         CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 33, TableNameEn = "FileStatuses",            TableNameAr = "حالات الملفات",         CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 34, TableNameEn = "SocialStatuses",          TableNameAr = "الحالات الاجتماعية",    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 35, TableNameEn = "HealthStatuses",          TableNameAr = "الحالات الصحية",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 36, TableNameEn = "TransactionStatuses",     TableNameAr = "حالات المعاملات",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 37, TableNameEn = "PensionFundBranches",     TableNameAr = "فروع الصندوق",          CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 38, TableNameEn = "SystemTables",            TableNameAr = "جداول النظام",          CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 39, TableNameEn = "SystemSettings",          TableNameAr = "إعدادات النظام",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 40, TableNameEn = "ReportTemplates",         TableNameAr = "قوالب التقارير",        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 41, TableNameEn = "SavedReports",            TableNameAr = "التقارير المحفوظة",     CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 42, TableNameEn = "RetirementTypes",         TableNameAr = "أنواع التقاعد",         CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 43, TableNameEn = "StatusTransitionLogs",    TableNameAr = "سجل تغيير الحالات",     CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SystemTable { TableID = 46, TableNameEn = "BudgetAllocations",    TableNameAr = "تخصيصات الميزانية",     CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
