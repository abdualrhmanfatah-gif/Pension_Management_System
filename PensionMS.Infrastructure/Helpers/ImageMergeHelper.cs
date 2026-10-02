using PensionMS.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PensionMS.Infrastructure.Helpers;

public static class ImageMergeHelper
{
    /// <summary>
    /// Merges front and back images into a single PDF if both exist.
    /// Returns a new Stream, ContentType and Extension.
    /// </summary>
    public static async Task<(Stream FileStream, string ContentType, string Extension)> MergeImagesToPdfAsync(IRemoteFile? front, IRemoteFile? back)
    {
        // Case 1: Neither
        if (front == null && back == null)
            return (Stream.Null, string.Empty, string.Empty);

        // Case 2: Only front
        if (front != null && back == null)
        {
            var ms = new MemoryStream();
            using var s = front.OpenReadStream();
            await s.CopyToAsync(ms);
            ms.Position = 0;
            return (ms, front.ContentType, Path.GetExtension(front.FileName));
        }

        // Case 3: Both (or just back)
        var outputStream = new MemoryStream();
        byte[]? frontBytes = front != null ? await GetBytesAsync(front) : null;
        byte[]? backBytes = back != null ? await GetBytesAsync(back) : null;

        Document.Create(container =>
        {
            if (frontBytes != null && frontBytes.Length > 0)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.Content().Image(frontBytes);
                });
            }

            if (backBytes != null && backBytes.Length > 0)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.Content().Image(backBytes);
                });
            }
        }).GeneratePdf(outputStream);

        outputStream.Position = 0;
        return (outputStream, "application/pdf", ".pdf");
    }

    private static async Task<byte[]> GetBytesAsync(IRemoteFile file)
    {
        using var s = file.OpenReadStream();
        using var ms = new MemoryStream();
        await s.CopyToAsync(ms);
        return ms.ToArray();
    }
}

public class StreamRemoteFile : IRemoteFile
{
    private readonly MemoryStream _stream;
    public string FileName { get; }
    public string ContentType { get; }
    public long Length => _stream.Length;

    public StreamRemoteFile(MemoryStream stream, string fileName, string contentType)
    {
        _stream = stream;
        FileName = fileName;
        ContentType = contentType;
    }

    public Stream OpenReadStream()
    {
        var copy = new MemoryStream(_stream.ToArray());
        return copy;
    }
}
