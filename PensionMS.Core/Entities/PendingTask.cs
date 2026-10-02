using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// مهمة معلقة ضمن مسار العمل (Workflow)
/// تُوجه لمستخدم محدد أو دور محدد
/// </summary>
public class PendingTask : ISoftDeletable
{
    public long Id { get; set; }

    /// <summary>
    /// معرّف الملف المرتبط بالمهمة (اختياري للمهام غير المرتبطة بملف، مثل اعتماد كشف معاش).
    /// </summary>
    public int? FileId { get; set; }
    public virtual RetirementFile? File { get; set; }

    /// <summary>
    /// معرّف كشف الصرف المرتبط بالمهمة (للمهام من نوع SalaryBatchApproval).
    /// </summary>
    public int? SalaryBatchID { get; set; }
    public virtual SalaryPaymentBatch? SalaryBatch { get; set; }

    /// <summary>
    /// نوع المهمة: MissingDocuments, FinancialReview, FinalApproval
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// معرّف المستخدم المسندة إليه المهمة (اختياري، إذا وُجهت لدور)
    /// </summary>
    public int? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }

    /// <summary>
    /// معرّف الدور المسندة إليه المهمة (اختياري، إذا وُجهت لمستخدم)
    /// </summary>
    public int? AssignedToRoleId { get; set; }
    public virtual Role? AssignedToRole { get; set; }

    /// <summary>
    /// حالة المهمة: Pending, Completed, Escalated
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// تاريخ الاستحقاق للمهمة
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// تاريخ إكمال المهمة (فارغ إذا لم تكتمل)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// معرّف المستخدم الذي أكمل المهمة
    /// </summary>
    public int? CompletedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }
}
