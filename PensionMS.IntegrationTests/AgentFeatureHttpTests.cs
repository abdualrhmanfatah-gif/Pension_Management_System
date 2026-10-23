using System.Net;
using System.Net.Http.Json;
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
using Moq;
using PensionMS.API.Controllers;
using PensionMS.API.Helpers;
using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.IntegrationTests;

public class AgentFeatureHttpTests : IAsyncLifetime
{
    private readonly Mock<IAgentService> _agents = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(_agents.Object);
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("test", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            foreach (var permission in new[] { "Agent.Add", "Agent.Edit", "Agent.View", "Agent.Delete" })
                options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
        });
        builder.Services.AddControllers(options => options.ModelBinderProviders.Insert(0, new RemoteFileModelBinderProvider()))
            .AddApplicationPart(typeof(AgentsController).Assembly);
        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        await _app.StartAsync();
        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [Fact]
    public async Task MultipartCreation_BindsBeneficiaryScope()
    {
        _agents.Setup(s => s.CreateAsync(It.IsAny<CreateAgentDto>(), 7)).ReturnsAsync((CreateAgentDto dto, int _) =>
            ApiResponse<AgentDto>.Success(new AgentDto { AgentID = 9, FileID = dto.FileID, EntitledID = dto.EntitledID, AgencyScope = dto.AgencyScope }));
        _client.DefaultRequestHeaders.Add("X-Test-Permission", "Agent.Add");
        using var form = new MultipartFormDataContent
        {
            { new StringContent("3"), "FileID" }, { new StringContent("5"), "EntitledID" },
            { new StringContent("3"), "AgencyScope" },
            { new StringContent("اسم الوكيل"), "FullName" }, { new StringContent("123456789"), "NationalID" }
        };
        using var response = await _client.PostAsync("/api/agents", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AgentDto>>();
        Assert.Equal(5, result!.Data!.EntitledID);
        Assert.Equal(3, result.Data.FileID);
        Assert.Equal(3, result.Data.AgencyScope);
    }

    [Fact]
    public async Task PhotoUpload_BindsFileWithoutSendingAgencyFields()
    {
        _agents.Setup(s => s.UpdatePhotoAsync(9, It.IsAny<IRemoteFile>(), 7)).ReturnsAsync(ApiResponse<string>.Success("photos/new.jpg"));
        _client.DefaultRequestHeaders.Add("X-Test-Permission", "Agent.Edit");
        using var form = new MultipartFormDataContent { { new ByteArrayContent(new byte[] { 1, 2, 3 }), "photo", "photo.jpg" } };
        using var response = await _client.PostAsync("/api/agents/9/photo", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _agents.Verify(s => s.UpdatePhotoAsync(9, It.Is<IRemoteFile>(f => f.Length == 3 && f.FileName == "photo.jpg"), 7), Times.Once);
        _agents.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateAgentDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ViewPermission_DoesNotAuthorizeCreation()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Permission", "Agent.View");
        using var form = new MultipartFormDataContent { { new StringContent("3"), "FileID" } };
        using var response = await _client.PostAsync("/api/agents", form);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _agents.Verify(s => s.CreateAsync(It.IsAny<CreateAgentDto>(), It.IsAny<int>()), Times.Never);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    public sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var permission = Request.Headers["X-Test-Permission"].ToString();
            if (string.IsNullOrEmpty(permission)) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "7"), new Claim("permission", permission) }, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }
}
