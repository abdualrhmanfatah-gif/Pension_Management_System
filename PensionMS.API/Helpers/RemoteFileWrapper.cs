using Microsoft.AspNetCore.Http;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Helpers;

public class RemoteFileWrapper : IRemoteFile
{
    private readonly IFormFile _file;

    public RemoteFileWrapper(IFormFile file)
    {
        _file = file;
    }

    public string FileName => _file.FileName;
    public string ContentType => _file.ContentType;
    public long Length => _file.Length;

    public Stream OpenReadStream() => _file.OpenReadStream();
}

public static class RemoteFileExtensions
{
    public static IRemoteFile ToRemoteFile(this IFormFile file) => new RemoteFileWrapper(file);
}
