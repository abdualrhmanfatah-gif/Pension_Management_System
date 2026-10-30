namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة لقطات الملفات (Versioning)
/// </summary>
public interface ISnapshotService
{
    Task CaptureAsync(int fileId, string snapshotType, int userId, string? reason = null);
}
