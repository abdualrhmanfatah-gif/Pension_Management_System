namespace PensionMS.Core.Entities;

/// <summary>
/// قواعد متطلبات الوثائق (عالمية أو مشروطة)
/// </summary>
public class DocumentRequirementRule
{
    public int RuleID { get; set; }

    public int DocumentTypeID { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;

    /// <summary>
    /// هل الوثيقة مطلوبة بشكل عام لكافة الملفات؟
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// الكيان المستهدف للفحص (Retiree, EntitledPerson, Agent, Guardian)
    /// </summary>
    public string? TargetEntity { get; set; }

    /// <summary>
    /// الحقل المراد فحصه (مثلاً: IsDisabled, Gender, Relationship)
    /// </summary>
    public string? ConditionField { get; set; }

    /// <summary>
    /// القيمة المطلوبة لتحقق الشرط (مثلاً: True, 1, Wife)
    /// </summary>
    public string? ConditionValue { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
