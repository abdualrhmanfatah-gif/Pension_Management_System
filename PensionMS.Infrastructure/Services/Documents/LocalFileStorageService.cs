using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Documents;

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

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName,
        string[]? allowedExtensions = null, long? maxSizeBytes = null)
    {
        // م30: سياسة رفع واحدة — قيم نوع الوثيقة تتقدم الافتراضي التاريخي
        var effectiveExtensions = allowedExtensions is { Length: > 0 }
            ? allowedExtensions
            : new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var effectiveMaxBytes = maxSizeBytes is > 0 ? maxSizeBytes.Value : 10L * 1024 * 1024;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!effectiveExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"نوع الملف غير مسموح: {extension}");

        if (fileStream.Length > effectiveMaxBytes)
            throw new InvalidOperationException(
                $"حجم الملف يتجاوز الحد المسموح ({effectiveMaxBytes / 1024d / 1024d:0.##} ميجابايت)");

        // م31: قراءة المحتوى مرة واحدة لفحص البنية كاملة (الملفات ضمن الحد المسموح أعلاه)
        var originalPosition = fileStream.Position;
        var content = new byte[fileStream.Length];
        var totalRead = 0;
        while (totalRead < content.Length)
        {
            var read = await fileStream.ReadAsync(content.AsMemory(totalRead, content.Length - totalRead));
            if (read == 0) break;
            totalRead += read;
        }
        fileStream.Position = originalPosition;

        if (totalRead != content.Length)
            throw new InvalidOperationException("تعذر قراءة محتوى الملف بالكامل");

        // م31: ترويسة magic bytes (مكافحة التضليل)
        var isValidContent = extension switch
        {
            ".pdf" => content.Length >= 4 && content[0] == 0x25 && content[1] == 0x50 && content[2] == 0x44 && content[3] == 0x46, // %PDF
            ".jpg" or ".jpeg" => content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF,             // FF D8 FF
            ".png" => content.Length >= 4 && content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47,   // ‰PNG
            _ => false
        };

        if (!isValidContent)
            throw new InvalidOperationException($"محتوى الملف لا يتطابق مع الامتداد {extension}");

        // م31: فحص بنية الملف الفعلية — 8 بايت ترويسة لا تثبت قابلية العرض
        ValidateStructure(content, extension);

        // specific folder inside uploads (e.g. "documents", "photos")
        var targetFolder = Path.Combine(_uploadsFolder, folderName);
        
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // م35: اسم تخزين مستقل تمامًا عن الاسم الأصلي — يمنع path traversal والأحرف الخطرة (# و% ومسافات)
        // الاسم الأصلي يبقى محفوظًا في قاعدة البيانات للاسم المعروض
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(targetFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await stream.WriteAsync(content, 0, content.Length);
        }

        // Return relative path from wwwroot (e.g. "uploads/documents/filename.ext")
        // standardized with forward slashes for web usage
        var relativePath = Path.Combine(Path.GetFileName(_uploadsFolder), folderName, uniqueFileName).Replace("\\", "/");
        return relativePath;
    }

    /// <summary>
    /// م31: فحص بنية الملف ضمن حدود موارد معروفة — يرفض الملفات التالفة غير القابلة للعرض
    /// </summary>
    private static void ValidateStructure(byte[] data, string extension)
    {
        switch (extension)
        {
            case ".png":
            {
                // التوقيع + طول IHDR (13) + "IHDR" + أبعاد غير صفرية + "IEND" ضمن آخر 16 بايت
                if (data.Length < 33
                    || data[8] != 0x00 || data[9] != 0x00 || data[10] != 0x00 || data[11] != 0x0D
                    || data[12] != (byte)'I' || data[13] != (byte)'H' || data[14] != (byte)'D' || data[15] != (byte)'R')
                    throw new InvalidOperationException("بنية PNG غير صالحة (مقطع IHDR مفقود)");

                var width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                var height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                if (width <= 0 || height <= 0)
                    throw new InvalidOperationException("بنية PNG غير صالحة (أبعاد غير صحيحة)");

                var hasIend = false;
                for (var i = Math.Max(0, data.Length - 16); i <= data.Length - 4; i++)
                {
                    if (data[i] == (byte)'I' && data[i + 1] == (byte)'E' && data[i + 2] == (byte)'N' && data[i + 3] == (byte)'D')
                    {
                        hasIend = true;
                        break;
                    }
                }
                if (!hasIend)
                    throw new InvalidOperationException("ملف PNG تالف (نقطة النهاية IEND مفقودة)");
                break;
            }

            case ".jpg":
            case ".jpeg":
            {
                if (data.Length < 4)
                    throw new InvalidOperationException("ملف JPEG تالف (أقصر من الحد الأدنى)");

                // EOI (FF D9) ضمن آخر 8 بايت — يكشف القص/الانقطاع
                var hasEoi = false;
                for (var i = Math.Max(0, data.Length - 8); i <= data.Length - 2; i++)
                {
                    if (data[i] == 0xFF && data[i + 1] == 0xD9)
                    {
                        hasEoi = true;
                        break;
                    }
                }
                if (!hasEoi)
                    throw new InvalidOperationException("ملف JPEG تالف (نقطة النهاية EOI مفقودة)");

                // SOF (C0-CF عدا C4/C8/CC) بأبعاد غير صفرية — يثبت وجود صورة حقيقية
                var hasSof = false;
                for (var i = 0; i + 8 < data.Length; i++)
                {
                    if (data[i] != 0xFF) continue;
                    var marker = data[i + 1];
                    if (marker < 0xC0 || marker > 0xCF || marker == 0xC4 || marker == 0xC8 || marker == 0xCC)
                        continue;
                    var h = (data[i + 5] << 8) | data[i + 6];
                    var w = (data[i + 7] << 8) | data[i + 8];
                    if (w > 0 && h > 0)
                    {
                        hasSof = true;
                        break;
                    }
                }
                if (!hasSof)
                    throw new InvalidOperationException("بنية JPEG غير صالحة (لم يُعثر على أبعاد الصورة)");
                break;
            }

            case ".pdf":
            {
                // %%EOF ضمن آخر 8KB — يكشف PDF المقطوع
                var tailLength = Math.Min(data.Length, 8192);
                var tail = System.Text.Encoding.ASCII.GetString(data, data.Length - tailLength, tailLength);
                if (!tail.Contains("%%EOF", StringComparison.Ordinal))
                    throw new InvalidOperationException("ملف PDF تالف (نقطة نهاية %%EOF مفقودة)");
                break;
            }
        }
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
