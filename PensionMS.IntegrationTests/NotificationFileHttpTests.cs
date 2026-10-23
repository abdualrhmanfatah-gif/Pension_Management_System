using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PensionMS.API.Controllers;
using PensionMS.API.Files;
using PensionMS.API.Middleware;
using PensionMS.Core.Interfaces;
using Moq;

namespace PensionMS.IntegrationTests;

/// <summary>Real HTTP routing and production file handlers, without a database or business files.</summary>
public sealed class NotificationFileHttpTests : IAsyncLifetime
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "PensionMS-NotificationFiles-" + Guid.NewGuid());
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private readonly Mock<IDocumentService> _documents = new();

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.Combine(_root, "uploads", "notifications"));
        Directory.CreateDirectory(Path.Combine(_root, "uploads", "documents"));
        await File.WriteAllTextAsync(Path.Combine(_root, "uploads", "notifications", "private.pdf"), "private fixture");
        await File.WriteAllTextAsync(Path.Combine(_root, "uploads", "documents", "public.pdf"), "public fixture");
        await File.WriteAllTextAsync(Path.Combine(_root, "asset.txt"), "static fixture");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = _root, EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddApplicationPart(typeof(FilesController).Assembly);
        _documents.Setup(d => d.CanDownloadAsync(It.IsAny<string>())).ReturnsAsync(true);
        builder.Services.AddSingleton(_documents.Object);
        builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("Test", _ => { });
        builder.Services.AddAuthorization();
        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.Map("/uploads", branch => branch.UseMiddleware<UploadsMiddleware>());
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = new PublicWebRootFileProvider(_root) });
        _app.MapControllers();
        await _app.StartAsync();
        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        _client = new HttpClient { BaseAddress = new Uri(address) };
        _client.DefaultRequestHeaders.Add("X-Test-User", "1");
    }

    [Theory]
    [InlineData("/api/files/notifications/private.pdf")]
    [InlineData("/api/files/documents%5C..%5Cnotifications%5Cprivate.pdf")]
    [InlineData("/api/files/documents%5C..%5CNOTIFICATIONS%5Cprivate.pdf")]
    [InlineData("/api/files/uploads%5Cnotifications%5Cprivate.pdf")]
    [InlineData("/api/files/notifications.%5Cprivate.pdf")]
    [InlineData("/uploads/notifications/private.pdf")]
    [InlineData("/uploads/notifications%5Cprivate.pdf")]
    [InlineData("/uploads/documents%5C..%5Cnotifications%5Cprivate.pdf")]
    [InlineData("/uploads/notifications.%5Cprivate.pdf")]
    [InlineData("/uploads/notifications%20%5Cprivate.pdf")]
    [InlineData("/uploads%5Cnotifications%5Cprivate.pdf")]
    [InlineData("/uploads.%5Cnotifications%5Cprivate.pdf")]
    public async Task LegacyRoutes_RejectNotificationAttachments(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("private fixture", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/files/documents/public.pdf")]
    [InlineData("/uploads/documents/public.pdf")]
    public async Task PublicDocuments_StillRequireAuthenticationAndAreServed(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("public fixture", await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.CacheControl!.Private);
        _client.DefaultRequestHeaders.Clear();
        using var anonymous = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [Theory]
    [InlineData("/api/files/documents/public.pdf")]
    [InlineData("/uploads/documents/public.pdf")]
    public async Task AuthenticatedUser_CannotDownloadOutsideTheAllowedScope(string path)
    {
        _documents.Setup(d => d.CanDownloadAsync(It.IsAny<string>())).ReturnsAsync(false);
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("public fixture", await response.Content.ReadAsStringAsync());
        _documents.Verify(d => d.CanDownloadAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task StaticAssetsArePublic_ButUploadsCannotBypassAuthentication()
    {
        _client.DefaultRequestHeaders.Clear();
        using var asset = await _client.GetAsync("/asset.txt");
        Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
        Assert.Equal("static fixture", await asset.Content.ReadAsStringAsync());
        using var upload = await _client.GetAsync("/uploads%5Cdocuments%5Cpublic.pdf");
        Assert.Equal(HttpStatusCode.NotFound, upload.StatusCode);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        // Only this fixture's independently created temporary directory is removed.
        Directory.Delete(_root, recursive: true);
    }

    private sealed class FixtureAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Test-User")) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "Test");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }
}
