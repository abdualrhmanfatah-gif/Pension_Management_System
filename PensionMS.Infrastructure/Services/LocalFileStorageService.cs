using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsFolder;
    private readonly string _baseUrl;

    public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        // Default to "uploads" inside wwwroot if not configured
        var folderName = config["FileStorage:UploadsFolder"] ?? "uploads";
        var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        _uploadsFolder = Path.Combine(webRootPath, folderName);
        _baseUrl = config["FileStorage:BaseUrl"] ?? ""; // e.g. "https://api.example.com" or empty for relative
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName)
    {
        // Defense-in-depth: validate extension and size
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"نوع الملف غير مسموح: {extension}");

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (fileStream.Length > maxFileSize)
            throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى (10 ميجابايت)");

        // Validate file content via magic bytes (anti-spoofing)
        var header = new byte[8];
        var originalPosition = fileStream.Position;
        await fileStream.ReadAsync(header, 0, 8);
        fileStream.Position = originalPosition;

        var isValidContent = extension switch
        {
            ".pdf" => header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46, // %PDF
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,             // FF D8 FF
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,   // ‰PNG
            _ => false
        };

        if (!isValidContent)
            throw new InvalidOperationException($"محتوى الملف لا يتطابق مع الامتداد {extension}");

        // specific folder inside uploads (e.g. "documents", "photos")
        var targetFolder = Path.Combine(_uploadsFolder, folderName);
        
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // Generate unique filename to avoid collisions
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(targetFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Return relative path from wwwroot (e.g. "uploads/documents/filename.ext")
        // standardized with forward slashes for web usage
        var relativePath = Path.Combine(Path.GetFileName(_uploadsFolder), folderName, uniqueFileName).Replace("\\", "/");
        return relativePath;
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Stream?> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }
        
        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public string GetFileUrl(string filePath)
    {
        // If BaseUrl is set (e.g. CDN or specific domain), prepend it
        // Otherwise return relative path (client handles base URL)
        if (string.IsNullOrEmpty(_baseUrl))
        {
            return $"/{filePath}";
        }
        return $"{_baseUrl}/{filePath}";
    }
}
