using PensionMS.Core.Entities;

namespace PensionMS.Core.Interfaces;

public interface IComplianceEngine
{
    /// <summary>
    /// يحسب قائمة بجميع أنواع الوثائق المطلوبة لملف معين بناءً على القواعد والمحتوى الحالي للملف
    /// </summary>
    Task<List<DocumentType>> GetRequiredDocumentTypesAsync(int fileId);

    /// <summary>
    /// يحسب ملخص حالة الامتثال (العدد الكلي للمطلوب، المرفوع، والناقص)
    /// </summary>
    Task<ComplianceSummary> GetComplianceSummaryAsync(int fileId);

    /// <summary>
    /// يقوم بتحديث الحقل المخزن (Cache) لعدد النواقص في جدول الملفات
    /// </summary>
    Task RefreshComplianceCacheAsync(int fileId);
}

public class ComplianceSummary
{
    public int TotalRequiredCount { get; set; }
    public int UploadedCount { get; set; }
    public int MissingCount { get; set; }
    public List<int> MissingDocumentTypeIds { get; set; } = new();
}
