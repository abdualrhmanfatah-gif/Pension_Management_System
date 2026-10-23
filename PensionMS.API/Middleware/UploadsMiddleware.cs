using PensionMS.API.Files;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Middleware;

public sealed class UploadsMiddleware
{
    private readonly string _uploadsRoot;

    public UploadsMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _uploadsRoot = Path.Combine(environment.WebRootPath, "uploads");
    }

    public async Task InvokeAsync(HttpContext context, IDocumentService documents)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!PublicUploadFiles.TryResolve(_uploadsRoot, context.Request.Path.Value ?? "", out var fullPath) ||
            !File.Exists(fullPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!await documents.CanDownloadAsync(context.Request.Path.Value ?? ""))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = PublicUploadFiles.ContentType(fullPath);
        context.Response.Headers["Cache-Control"] = "private, no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        await context.Response.SendFileAsync(fullPath, context.RequestAborted);
    }
}
