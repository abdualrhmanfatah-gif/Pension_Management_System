using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PensionMS.API.Controllers;
using PensionMS.API.Security;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.IntegrationTests;

public class ApprovalPermissionHttpTests
{
    [Theory]
    [InlineData("POST", "/api/retirement-files/5/review/approve", "File.Review.Approve")]
    [InlineData("POST", "/api/retirement-files/review-items/8/approve", "File.Review.Approve")]
    [InlineData("PUT", "/api/salary-batches/5/approve", "SalaryBatch.Approve")]
    public async Task Approval_RequiresItsPermission_AndUsesAuthenticatedActor(string method, string path, string permission)
    {
        var review = new Mock<IFileReviewService>();
        review.Setup(s => s.ApproveReviewAsync(5, 7, null)).ReturnsAsync(ApiResponse<bool>.Success(true));
        review.Setup(s => s.ApproveItemAsync(8, 7, null)).ReturnsAsync(ApiResponse<bool>.Success(true));
        var payroll = new Mock<ISalaryPaymentBatchService>();
        payroll.Setup(s => s.ApproveAsync(5, 7)).ReturnsAsync(ApiResponse<bool>.Success(true));
        var permissions = new Mock<IScopedPermissionResolver>();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton(Mock.Of<IRetirementFileService>());
        builder.Services.AddSingleton(Mock.Of<IFileQueryService>());
        builder.Services.AddSingleton(Mock.Of<IRetirementFileReportService>());
        builder.Services.AddSingleton(Mock.Of<IFileStatusService>());
        builder.Services.AddSingleton(Mock.Of<IDocumentService>());
        builder.Services.AddSingleton(review.Object);
        builder.Services.AddSingleton(payroll.Object);
        builder.Services.AddSingleton(permissions.Object);
        builder.Services.AddSingleton<ExcelReportTemplate>();
        builder.Services.AddSingleton<ExcelReportExporter>();
        builder.Services.AddSingleton<UploaderStatsReportExporter>();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions,
            AgentFeatureHttpTests.TestAuthenticationHandler>("test", _ => { });
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))));
        builder.Services.AddControllers().AddApplicationPart(typeof(RetirementFilesController).Assembly);
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };

        using (var anonymous = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)))
            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-Permission", "ViewOnly");
        using (var denied = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)))
            Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        review.Verify(s => s.ApproveReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        review.Verify(s => s.ApproveItemAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        payroll.Verify(s => s.ApproveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

        permissions.Setup(s => s.HasPermissionAsync(7, permission)).ReturnsAsync(true);
        using var allowed = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        if (path.Contains("review-items")) review.Verify(s => s.ApproveItemAsync(8, 7, null), Times.Once);
        else if (method == "POST") review.Verify(s => s.ApproveReviewAsync(5, 7, null), Times.Once);
        else payroll.Verify(s => s.ApproveAsync(5, 7), Times.Once);
    }
}
