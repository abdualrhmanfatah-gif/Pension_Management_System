namespace PensionMS.Core.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// حفظ ملف مع سياسة تحقق موحدة (م30):
    /// allowedExtensions/maxSizeBytes تصل من نوع الوثيقة؛ الافتراضي يطابق السياسة التاريخية (PDF/JPG/PNG و10MB)
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName,
        string[]? allowedExtensions = null, long? maxSizeBytes = null);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    string GetFileUrl(string filePath);
}
