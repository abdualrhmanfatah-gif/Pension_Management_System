using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Files;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        }
    }
}
