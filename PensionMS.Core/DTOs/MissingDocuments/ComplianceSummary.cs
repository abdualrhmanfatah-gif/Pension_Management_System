namespace PensionMS.Core.DTOs.MissingDocuments;

/// <summary>عدد متطلبات الوثائق والمرفوع الصحيح منها؛ التعهدات والإعفاءات لا تُعد وثائق مرفوعة.</summary>
public sealed class ComplianceSummary
{
    public int TotalRequiredCount { get; set; }
    public int UploadedCount { get; set; }
    public int MissingCount { get; set; }
    public List<int> MissingDocumentTypeIds { get; set; } = new();
}
