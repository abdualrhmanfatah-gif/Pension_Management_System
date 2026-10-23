namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة التدقيق المقاوم للتلاعب (Hash-Chained)
/// </summary>
public interface IImmutableAuditService
{
    Task LogAsync(
        int actorId,
        string actorName,
        string action,
        string entityType,
        int entityId,
        string? oldValues,
        string? newValues,
        int? branchId = null,
        string? ipAddress = null,
        Guid? correlationId = null);

    /// <summary>
    /// تحقق من سلامة سلسلة التدقيق — يرجع عدد مخالفات الارتباط والتجزئة؛ قد يُحسب السجل مرتين
    /// </summary>
    Task<int> VerifyChainIntegrityAsync();
}
