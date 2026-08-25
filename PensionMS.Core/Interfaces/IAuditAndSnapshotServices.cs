using PensionMS.Core.Entities;

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
    /// تحقق من سلامة سلسلة التدقيق — يرجع عدد السجلات الفاشلة
    /// </summary>
    Task<int> VerifyChainIntegrityAsync();
}

/// <summary>
/// خدمة لقطات الملفات (Versioning)
/// </summary>
public interface ISnapshotService
{
    Task CaptureAsync(int fileId, string snapshotType, int userId, string? reason = null);
}
