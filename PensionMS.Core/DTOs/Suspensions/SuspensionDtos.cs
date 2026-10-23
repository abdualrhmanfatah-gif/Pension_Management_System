namespace PensionMS.Core.DTOs.Suspensions;

/// <summary>نتيجة فحص موانع الصرف؛ لا تغني عن بقية فحوص تنفيذ العملية المالية.</summary>
public sealed record PaymentHoldCheckDto(bool IsHeld, string? Reason, string SalaryStatus);

/// <summary>
/// DTO لعرض بيانات الإيقاف
/// </summary>
public class SuspensionDto
{
    public int SuspensionID { get; set; }
    public int FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? RetireeName { get; set; }
    public string? NationalID { get; set; }
    public string? MilitaryNumber { get; set; }
    public DateTime SuspensionDate { get; set; }
    public string SuspensionReason { get; set; } = string.Empty;
    public string SuspensionSource { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public string? EndReason { get; set; }
    public string? CreatedByUser { get; set; }
    public string? EndedByUser { get; set; }
    public DateTime CreatedAt { get; set; }

    // Linked pledge (if any)
    public PledgeSummaryDto? LinkedPledge { get; set; }
}

/// <summary>
/// DTO لعرض ملخص التعهد
/// </summary>
public class PledgeSummaryDto
{
    public int PledgeID { get; set; }
    public DateTime PledgeDate { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? FulfilledAt { get; set; }
    public string PledgedByType { get; set; } = string.Empty;
    public string? PledgedByName { get; set; }
    public int MissingDocumentCount { get; set; }
}

/// <summary>
/// DTO لإنشاء إيقاف مع تعهد اختياري
/// </summary>
public class CreateSuspensionDto
{
    public int FileID { get; set; }
    public string SuspensionReason { get; set; } = string.Empty;
    public string SuspensionSource { get; set; } = string.Empty;

    // Pledge fields (optional — if missing docs need a pledge)
    public List<int>? MissingDocumentIDs { get; set; }
    public int? DeadlineDays { get; set; }
    public string? PledgedByType { get; set; }
    public string? PledgedByName { get; set; }
}

/// <summary>
/// DTO لإنهاء الإيقاف
/// </summary>
public class EndSuspensionDto
{
    public string EndReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO لإنشاء إعفاء وثيقة
/// </summary>
public class CreateExemptionDto
{
    public int FileID { get; set; }
    public List<int> MissingDocumentIDs { get; set; } = new();
    public string DirectiveSource { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO لعرض إعفاء الوثيقة
/// </summary>
public class ExemptionDto
{
    public int ExemptionID { get; set; }
    public int FileID { get; set; }
    public string DirectiveSource { get; set; } = string.Empty;
    public DateTime ExemptionDate { get; set; }
    public string? Notes { get; set; }
    public string? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExemptedDocumentCount { get; set; }
}
