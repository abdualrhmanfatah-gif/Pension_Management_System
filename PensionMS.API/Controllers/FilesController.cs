<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Files;
using PensionMS.Core.Interfaces;
=======
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
<<<<<<< HEAD
[Authorize]
public class FilesController : ControllerBase
{
    private readonly string _uploadsPath;
    private readonly IDocumentService _documents;

    public FilesController(IWebHostEnvironment env, IDocumentService documents)
    {
        _uploadsPath = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));
        _documents = documents;
    }

    [HttpGet("{*filePath}")]
    public async Task<IActionResult> GetFile(string filePath)
    {
        try
        {
            if (!PublicUploadFiles.TryResolve(_uploadsPath, filePath, out var fullPath) ||
                !System.IO.File.Exists(fullPath))
                return NotFound();

            if (!await _documents.CanDownloadAsync(filePath)) return NotFound();

            Response.Headers["Cache-Control"] = "private, no-store";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(fullPath, PublicUploadFiles.ContentType(fullPath));
        }
        catch
        {
            return StatusCode(500, new { error = "تعذر قراءة الملف" });
=======
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
        _uploadsPath = Path.Combine(env.WebRootPath, "uploads");
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

            if (!fullPath.StartsWith(_uploadsPath))
                return BadRequest("Invalid path");

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            Response.Headers["Cache-Control"] = "private, max-age=86400";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(fullPath, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, path = filePath });
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }
    }
}
