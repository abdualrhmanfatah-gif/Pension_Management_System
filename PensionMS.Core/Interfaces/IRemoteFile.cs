namespace PensionMS.Core.Interfaces;

/// <summary>
/// Abstraction for a file to keep Core framework-agnostic.
/// Implemented by IFormFile wrappers in API or physical file wrappers in Infrastructure.
/// </summary>
public interface IRemoteFile
{
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
    Stream OpenReadStream();
}
