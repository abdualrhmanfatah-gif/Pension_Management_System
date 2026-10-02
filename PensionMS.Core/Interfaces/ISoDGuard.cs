namespace PensionMS.Core.Interfaces;

/// <summary>
/// نتيجة فحص تعارض المصالح (SoD)
/// </summary>
public class SoDResult
{
    public bool IsAllowed { get; private set; }
    public string? Reason { get; private set; }

    public static SoDResult Ok() => new() { IsAllowed = true };
    public static SoDResult Fail(string reason) => new() { IsAllowed = false, Reason = reason };
}

/// <summary>
/// سجل فعل تاريخي على ملف — يُستخدم لفحص SoD بناءً على التاريخ
/// </summary>
public class FileActionRecord
{
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// الإجراءات التي يتم فحص SoD عليها
/// </summary>
public enum SoDAction
{
    Approve,
    Activate,
    Override,
    BatchApprove
}

/// <summary>
/// قاعدة SoD واحدة — قابلة للتوسع عبر DI
/// </summary>
public interface ISoDRule
{
    Task<SoDResult> EvaluateAsync(
        int actorId,
        SoDAction action,
        Core.Entities.RetirementFile file,
        List<FileActionRecord> history);
}

/// <summary>
/// محرك SoD الرئيسي — يجمع كل القواعد ويطبقها
/// </summary>
public interface ISoDGuard
{
    Task<SoDResult> ValidateAsync(int fileId, int actorId, SoDAction action);

    /// <summary>
    /// يتحقق من أن المستخدم مسموح له باعتماد كشف المعاش (مثلاً: ليس منشئ الكشف).
    /// </summary>
    Task<SoDResult> ValidateSalaryBatchApprovalAsync(int batchId, int actorId);
}
