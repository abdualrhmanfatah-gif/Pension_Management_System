using Microsoft.AspNetCore.StaticFiles;

namespace PensionMS.API.Files;

/// <summary>Shared policy for legacy file routes; notification attachments require their ownership endpoint.</summary>
public static class PublicUploadFiles
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt"
    };

    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public static bool TryResolve(string uploadsRoot, string requestedPath, out string fullPath)
    {
        fullPath = "";
        try
        {
            var relative = requestedPath.Replace('\\', '/').TrimStart('/');
            if (relative.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                relative = relative["uploads/".Length..];

            // Windows trims trailing spaces/dots and accepts alternate data streams.
            if (relative.Split('/').Any(segment => segment.Contains(':') ||
                (segment is not "." and not ".." && (segment.EndsWith('.') || segment.EndsWith(' ')))))
                return false;

            var root = Path.GetFullPath(uploadsRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var candidate = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            var protectedRoot = Path.Combine(root, "notifications") + Path.DirectorySeparatorChar;

            if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(protectedRoot, StringComparison.OrdinalIgnoreCase) ||
                !Extensions.Contains(Path.GetExtension(candidate)))
                return false;

            fullPath = candidate;
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    public static string ContentType(string path) =>
        ContentTypes.TryGetContentType(path, out var contentType) ? contentType : "application/octet-stream";
}
