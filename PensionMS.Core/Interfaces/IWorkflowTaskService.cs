namespace PensionMS.Core.Interfaces;

/// <summary>
/// نموذج محرك سير العمل (Workflow Engine)
/// يتولى التوجيه التلقائي للملفات إلى الأقسام والمستخدمين المعنيين بناءً على الحالة والقواعد
/// </summary>
public interface IWorkflowTaskService
{
    /// <summary>
    /// يوجّه الملف إلى المحطة التالية وينشئ مهمة معلقة (PendingTask)
    /// </summary>
    Task RouteFileAsync(int fileId, string action, int currentUserId);

    /// <summary>
    /// ينشئ مهمة اعتماد كشف معاش في طابور المالية.
    /// </summary>
    Task CreateSalaryBatchApprovalTaskAsync(int batchId, DateTime batchMonth);

    /// <summary>
    /// يغلق مهام اعتماد كشف المعاش المرتبطة بالكشف بعد الاعتماد.
    /// </summary>
    Task ResolveSalaryBatchApprovalTasksAsync(int batchId, int completedByUserId);
}
