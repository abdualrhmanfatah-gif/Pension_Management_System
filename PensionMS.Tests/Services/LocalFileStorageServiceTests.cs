using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using PensionMS.Infrastructure.Services;
using System.Text;

namespace PensionMS.Tests.Services;

/// <summary>
/// اختبارات موحدة السياسة (م30) وفحص بنية الملف (م31) واسم التخزين المستقل (م35)
/// </summary>
public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _webRoot;

    public LocalFileStorageServiceTests()
    {
        _webRoot = Path.Combine(Path.GetTempPath(), "pensionms-storage-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_webRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_webRoot, true); } catch { /* تجاهل */ }
    }

    private LocalFileStorageService CreateService()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(_webRoot);
        env.Setup(e => e.ContentRootPath).Returns(_webRoot);

        var config = new Mock<IConfiguration>();
        config.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);

        return new LocalFileStorageService(env.Object, config.Object);
    }

    // ─── بناة ملفات اصطناعية ─────────────────────────────

    /// <summary>PNG صالح يُرضي مدقق البنية: توقيع + IHDR بأبعاد + IEND</summary>
    private static byte[] BuildValidPng(int width = 16, int height = 16)
    {
        using var ms = new MemoryStream();
        void W(params byte[] b) => ms.Write(b, 0, b.Length);
        W(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A); // توقيع PNG
        W(0x00, 0x00, 0x00, 0x0D);                          // طول IHDR = 13
        W((byte)'I', (byte)'H', (byte)'D', (byte)'R');
        W((byte)(width >> 24), (byte)(width >> 16), (byte)(width >> 8), (byte)width);
        W((byte)(height >> 24), (byte)(height >> 16), (byte)(height >> 8), (byte)height);
        W(0x08, 0x02, 0x00, 0x00, 0x00);                    // bit depth/color/compression/filter/interlace
        W(0x00, 0x00, 0x00, 0x00);                          // CRC (وهمي — المدقق لا يفحصه)
        W(0x00, 0x00, 0x00, 0x00, (byte)'I', (byte)'E', (byte)'N', (byte)'D', 0xAE, 0x42, 0x60, 0x82);
        return ms.ToArray();
    }

    /// <summary>JPEG صالح: SOI + SOF0 بأبعاد + EOI</summary>
    private static byte[] BuildValidJpeg(bool includeEoi = true)
    {
        using var ms = new MemoryStream();
        void W(params byte[] b) => ms.Write(b, 0, b.Length);
        W(0xFF, 0xD8);                                     // SOI
        W(0xFF, 0xC0, 0x00, 0x11, 0x08);                   // SOF0 طول=17 دقة=8
        W(0x00, 0x40);                                     // ارتفاع 64
        W(0x00, 0x50);                                     // عرض 80
        W(0x03, 0x01, 0x11, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01);
        var filler = new byte[64];
        Random.Shared.NextBytes(filler);
        W(filler);
        if (includeEoi) W(0xFF, 0xD9);                     // EOI
        return ms.ToArray();
    }

    private static byte[] BuildPdf(bool includeEof = true)
    {
        var sb = new StringBuilder("%PDF-1.4\n1 0 obj\n<< /Type /Catalog >>\nendobj\ntrailer\n");
        if (includeEof) sb.Append("%%EOF\n");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    // ─── م31: فحص البنية ─────────────────────────────────

    [Fact]
    public async Task SaveFileAsync_RejectsFakePng_WithOnlyHeader()
    {
        // ملف PNG وهمي من 8 بايت يبدأ بالتوقيع المتوقع — كان يُحفظ بنجاح قبل الإصلاح
        var fake = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(fake), "fake.png", "documents"));

        Assert.Contains("PNG", ex.Message);
    }

    [Fact]
    public async Task SaveFileAsync_RejectsPngWithoutIend()
    {
        var valid = BuildValidPng();
        var truncated = valid.AsSpan(0, valid.Length - 12).ToArray(); // بلا IEND
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(truncated), "truncated.png", "documents"));

        Assert.Contains("IEND", ex.Message);
    }

    [Fact]
    public async Task SaveFileAsync_AcceptsValidPng()
    {
        var service = CreateService();

        var path = await service.SaveFileAsync(new MemoryStream(BuildValidPng()), "photo.png", "documents");

        Assert.Matches(@"^uploads/documents/[0-9a-f]{32}\.png$", path);
        Assert.True(File.Exists(Path.Combine(_webRoot, path.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task SaveFileAsync_RejectsJpegWithoutEoi()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(BuildValidJpeg(includeEoi: false)), "cut.jpg", "documents"));

        Assert.Contains("EOI", ex.Message);
    }

    [Fact]
    public async Task SaveFileAsync_AcceptsValidJpeg()
    {
        var service = CreateService();

        var path = await service.SaveFileAsync(new MemoryStream(BuildValidJpeg()), "photo.jpg", "documents");

        Assert.EndsWith(".jpg", path);
    }

    [Fact]
    public async Task SaveFileAsync_RejectsTruncatedPdf_WithoutEof()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(BuildPdf(includeEof: false)), "cut.pdf", "documents"));

        Assert.Contains("PDF", ex.Message);
    }

    // ─── م30: سياسة موحدة من نوع الوثيقة ─────────────────

    [Fact]
    public async Task SaveFileAsync_HonorsPolicyExtensions_OverDefault()
    {
        // السياسة تقول JPG فقط — PNG كان سيمر عبر الافتراضي القديم
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(BuildValidPng()), "photo.png", "documents",
                allowedExtensions: new[] { ".jpg" }));

        Assert.Contains("غير مسموح", ex.Message);
    }

    [Fact]
    public async Task SaveFileAsync_HonorsPolicyMaxBytes_OverDefault()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(BuildValidPng()), "photo.png", "documents",
                maxSizeBytes: 10));

        Assert.Contains("حجم الملف", ex.Message);
    }

    [Fact]
    public async Task SaveFileAsync_PolicyAllowsExtensions_BeyondHistoricalDefault()
    {
        // WebP ضمن allowedExtensions لبعض الأنواع — تجاوز القيود الافتراضية الجامدة (م30)
        var service = CreateService();
        var fakeWebp = new byte[64];
        fakeWebp[0] = 0x52; fakeWebp[1] = 0x49; fakeWebp[2] = 0x46; fakeWebp[3] = 0x46; // RIFF
        fakeWebp[8] = 0x57; fakeWebp[9] = 0x45; fakeWebp[10] = 0x42; fakeWebp[11] = 0x50; // WEBP

        // الافتراضي القديم كان يرفض .webp برمجية InvalidOperationException قبل الوصول للمحتوى
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveFileAsync(new MemoryStream(fakeWebp), "photo.webp", "documents",
                allowedExtensions: new[] { ".webp" }));
        // م05: المحتوى لا يتطابق (الامتداد مسموح الآن) — الرفض يكون على المحتوى لا الامتداد
        Assert.Contains("محتوى الملف", ex.Message);
    }

    // ─── م35: اسم تخزين مستقل ────────────────────────────

    [Fact]
    public async Task SaveFileAsync_StorageNameDropsOriginalName()
    {
        var service = CreateService();

        var path = await service.SaveFileAsync(
            new MemoryStream(BuildValidPng()), "تقرير #1 %.png", "documents");

        var storedName = Path.GetFileName(path);
        Assert.Matches(@"^[0-9a-f]{32}\.png$", storedName);
        Assert.DoesNotContain("#", storedName);
        Assert.DoesNotContain("%", storedName);
    }
}
