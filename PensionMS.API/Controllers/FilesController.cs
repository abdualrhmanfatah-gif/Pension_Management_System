using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        }
    }
}
