namespace PensionMS.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    string GetFileUrl(string filePath);
}
