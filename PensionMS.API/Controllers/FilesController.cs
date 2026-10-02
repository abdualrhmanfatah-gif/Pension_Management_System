using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt"
    };

    private readonly string _uploadsPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FilesController(IWebHostEnvironment env)
    {
        _uploadsPath = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    [HttpGet("{*filePath}")]
    public IActionResult GetFile(string filePath)
    {
        try
        {
            var normalizedPath = filePath.Replace('\\', '/').TrimStart('/');
            if (normalizedPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                normalizedPath = normalizedPath["uploads/".Length..];

            var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, normalizedPath));

            // مقارنة حد المجلد بالفاصل الكامل لمنع المجلدات المجاورة مثل uploads-backup
            var uploadsRoot = _uploadsPath.EndsWith(Path.DirectorySeparatorChar)
                ? _uploadsPath
                : _uploadsPath + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid path");

            if (!AllowedExtensions.Contains(Path.GetExtension(fullPath)))
                return NotFound();

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            Response.Headers["Cache-Control"] = "private, max-age=86400";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(fullPath, contentType);
        }
        catch
        {
            return StatusCode(500, new { error = "تعذر قراءة الملف" });
        }
    }
}
