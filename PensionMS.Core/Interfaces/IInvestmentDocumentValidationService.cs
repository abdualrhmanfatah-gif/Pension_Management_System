using PensionMS.Core.Entities;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة التحقق من جاهزية الوثائق الاستثمارية — التحقق من اكتمال المستندات المطلوبة لكل كيان
/// </summary>
public interface IInvestmentDocumentValidationService
{
    /// <summary>
    /// استرجاع حالة جاهزية الوثائق لكيان معين (عام)
    /// </summary>
    Task<ApiResponse<DocumentReadinessDto>> GetDocumentReadinessAsync(DocumentOwnerType ownerType, int entityId);

    /// <summary>
    /// التحقق مما إذا كانت جميع الوثائق الإلزامية مكتملة قبل تغيير الحالة
    /// </summary>
    Task<ApiResponse<DocumentValidationResultDto>> ValidateReadinessForStatusChangeAsync(DocumentOwnerType ownerType, int entityId);
}

/// <summary>
/// DTO لعرض حالة جاهزية الوثائق
/// </summary>
public class DocumentReadinessDto
{
    public int TotalRequired { get; set; }
    public int TotalProvided { get; set; }
    public int TotalMissing { get; set; }
    public int TotalExpired { get; set; }
    public decimal CompletionPercentage { get; set; }
    public List<DocumentReadinessItemDto> Items { get; set; } = new();
}

public class DocumentReadinessItemDto
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? TypeNameEn { get; set; }
    public string SystemCode { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool HasExpiry { get; set; }

    /// <summary>
    /// الحالة: Provided, Missing, Expired
    /// </summary>
    public string Status { get; set; } = "Missing";
    public DateTime? ExpiryDate { get; set; }
    public DateTime? UploadedAt { get; set; }
}

/// <summary>
/// DTO لنتيجة التحقق من الجاهزية
/// </summary>
public class DocumentValidationResultDto
{
    public bool IsReady { get; set; }
    public int MissingCount { get; set; }
    public int ExpiredCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DocumentReadinessItemDto> MissingDocuments { get; set; } = new();
    public List<DocumentReadinessItemDto> ExpiredDocuments { get; set; } = new();
}
