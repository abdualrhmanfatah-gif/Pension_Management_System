using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace PensionMS.API.Files;

/// <summary>Static assets must never expose uploads, including Windows separator aliases.</summary>
public sealed class PublicWebRootFileProvider : IFileProvider, IDisposable
{
    private readonly PhysicalFileProvider _inner;
    private readonly string _root;
    private readonly string _uploads;

    public PublicWebRootFileProvider(string root)
    {
        _inner = new PhysicalFileProvider(root);
        _root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        _uploads = Path.Combine(_root, "uploads");
    }

    private bool IsPublic(string subpath)
    {
        try
        {
            var relative = subpath.Replace('\\', '/').TrimStart('/');
            if (relative.Split('/').Any(segment => segment.Contains(':') ||
                (segment is not "." and not ".." && (segment.EndsWith('.') || segment.EndsWith(' ')))))
                return false;
            var fullPath = Path.GetFullPath(Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar)));
            return fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.Equals(_uploads, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.StartsWith(_uploads + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    public IFileInfo GetFileInfo(string subpath) =>
        IsPublic(subpath) ? _inner.GetFileInfo(subpath) : new NotFoundFileInfo(subpath);

    public IDirectoryContents GetDirectoryContents(string subpath) =>
        IsPublic(subpath) ? _inner.GetDirectoryContents(subpath) : NotFoundDirectoryContents.Singleton;

    public IChangeToken Watch(string filter) => _inner.Watch(filter);
    public void Dispose() => _inner.Dispose();
}
