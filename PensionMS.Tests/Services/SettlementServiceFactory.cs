using PensionMS.Infrastructure.Services.Settlements;
using PensionMS.Infrastructure.Reporting.Exporters;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Tests.Services;

/// <summary>
/// نقطة بناء وحيدة لـ <see cref="SettlementService"/>. الغرض: إضافة dependency جديدة للخدمة
/// تتطلب تعديل هذا الملف فقط بدل كل ملف اختبار يستدعي الـ constructor يدوياً.
/// </summary>
internal static class SettlementServiceFactory
{
    public sealed record Dependencies
    {
        public required Mock<IUnitOfWork> UnitOfWork { get; init; }
        public Mock<ISettlementRepository> Settlements { get; init; } = new();
        public Mock<IFinancialEntitlementService> Entitlements { get; init; } = new();
        public Mock<INotificationService> Notifications { get; init; } = new();
        public Mock<IJournalEntryService> JournalEntry { get; init; } = new();
        public Mock<IImmutableAuditService> Audit { get; init; } = new();
        public Mock<IMissingDocumentService> Compliance { get; init; } = new();
        public IMemoryCache Cache { get; init; } = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>يبني الخدمة مع mocks افتراضية، مع إمكانية تجاوز أي dependency.</summary>
    public static SettlementService Create(
        Mock<IUnitOfWork> unitOfWork,
        Mock<ISettlementRepository>? settlements = null,
        Mock<IFinancialEntitlementService>? entitlements = null,
        Mock<INotificationService>? notifications = null,
        Mock<IJournalEntryService>? journalEntry = null,
        Mock<IImmutableAuditService>? audit = null,
        Mock<IMissingDocumentService>? compliance = null,
        IMemoryCache? cache = null)
    {
        var deps = new Dependencies
        {
            UnitOfWork = unitOfWork,
            Settlements = settlements ?? new Mock<ISettlementRepository>(),
            Entitlements = entitlements ?? new Mock<IFinancialEntitlementService>(),
            Notifications = notifications ?? new Mock<INotificationService>(),
            JournalEntry = journalEntry ?? new Mock<IJournalEntryService>(),
            Audit = audit ?? new Mock<IImmutableAuditService>(),
            Compliance = compliance ?? new Mock<IMissingDocumentService>(),
            Cache = cache ?? new MemoryCache(new MemoryCacheOptions())
        };

        return new SettlementService(
            deps.UnitOfWork.Object,
            deps.Settlements.Object,
            deps.Entitlements.Object,
            deps.Notifications.Object,
            deps.JournalEntry.Object,
            deps.Audit.Object,
            deps.Cache,
            deps.Compliance.Object,
            new ExcelReportExporter(new ExcelReportTemplate()), PensionMS.Testing.TestPensionAccess.All(deps.UnitOfWork.Object));
    }
}
