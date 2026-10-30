using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Services.Investment;

namespace PensionMS.IntegrationTests;

public class InvestmentLeasingAuthorizationHttpTests
{
    [Theory]
    [InlineData(false, true, HttpStatusCode.Unauthorized)]
    [InlineData(true, false, HttpStatusCode.Forbidden)]
    [InlineData(true, true, HttpStatusCode.Forbidden)]
    public async Task CreateTenant_RequiresAuthenticatedCurrentOperationBeforeRepositoryAccess(
        bool authenticated, bool allowed, HttpStatusCode expected)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var permissions = Permissions(allowed);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "Investment.Manage")).ReturnsAsync(PermissionScope.Denied);
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(unit.Object);
            services.AddSingleton(permissions.Object);
            services.AddScoped<IInvestmentLeasingService, InvestmentLeasingService>();
            services.AddSingleton(Mock.Of<IFileStorageService>());
        });
        using var client = InvestmentProjectHttpHost.Client(app, authenticated ? 7 : null);
        using var response = await client.PostAsJsonAsync("/api/investment/leasing/tenants?userId=999", new CreateTenantDto
        { FullName = "مستأجر اختبار", NationalIDOrCR = "synthetic-tenant" });
        Assert.Equal(expected, response.StatusCode);
        if (authenticated && allowed)
        {
            permissions.Verify(p => p.GetAllowedScopeAsync(7, "Investment.Manage"), Times.Once);
            Assert.False((await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.IsSuccess);
        }
        unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HiddenContract_ReturnsExistingNotFoundApiResponse()
    {
        var service = new Mock<IInvestmentLeasingService>(MockBehavior.Strict);
        service.Setup(s => s.GetLeaseContractByIdAsync(2)).ReturnsAsync(ApiResponse<LeaseContractDto>.Fail("العقد غير موجود"));
        await using var app = await MockServiceApp(service.Object, Mock.Of<IFileStorageService>());
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = await client.GetAsync("/api/investment/leasing/contracts/2");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LeaseContractDto>>();
        Assert.False(body!.IsSuccess);
        Assert.Null(body.Data);
        Assert.Equal("العقد غير موجود", body.Message);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task UploadSignedContract_PreflightRejectsBeforeStorage(bool forbidden, HttpStatusCode expected)
    {
        var service = new Mock<IInvestmentLeasingService>(MockBehavior.Strict);
        if (forbidden) service.Setup(s => s.GetLeaseContractByIdAsync(2)).ThrowsAsync(new BranchScopeAccessException());
        else service.Setup(s => s.GetLeaseContractByIdAsync(2)).ReturnsAsync(ApiResponse<LeaseContractDto>.Fail("العقد غير موجود"));
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        await using var app = await MockServiceApp(service.Object, storage.Object);
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var content = ContractFile();
        using var response = await client.PostAsync("/api/investment/leasing/contracts/2/upload-signed-contract", content);
        Assert.Equal(expected, response.StatusCode);
        if (!forbidden)
        {
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.False(body!.IsSuccess);
            Assert.False(body.Data);
        }
        service.Verify(s => s.UpdateLeaseContractFilePathAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        storage.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("success", HttpStatusCode.OK, true)]
    [InlineData("result-failure", HttpStatusCode.BadRequest, false)]
    [InlineData("exception", HttpStatusCode.BadRequest, false)]
    [InlineData("revoked", HttpStatusCode.Forbidden, false)]
    [InlineData("cleanup-failure", HttpStatusCode.InternalServerError, true)]
    [InlineData("result-cleanup-failure", HttpStatusCode.InternalServerError, true)]
    [InlineData("already-absent", HttpStatusCode.BadRequest, false)]
    public async Task UploadSignedContract_AuthorizesBeforeStorage_RechecksOnUpdate_AndCompensatesFailures(
        string outcome, HttpStatusCode expected, bool newFileRemains)
    {
        var folder = Path.Combine(Path.GetTempPath(), "PensionMS_LeaseUpload_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var newFile = Path.Combine(folder, "new.pdf");
        var oldFile = Path.Combine(folder, "old.pdf");
        await File.WriteAllBytesAsync(oldFile, [1, 2, 3]);
        const string relativePath = "uploads/contracts/new.pdf";
        var sequence = new List<string>();
        var service = new Mock<IInvestmentLeasingService>(MockBehavior.Strict);
        service.Setup(s => s.GetLeaseContractByIdAsync(1)).Callback(() => sequence.Add("authorize"))
            .ReturnsAsync(ApiResponse<LeaseContractDto>.Success(new LeaseContractDto
            { ContractID = 1, SignedContractPath = "uploads/contracts/old.pdf" }));
        service.Setup(s => s.UpdateLeaseContractFilePathAsync(1, relativePath, 7)).ReturnsAsync(() =>
        {
            sequence.Add("update");
            if (outcome is "exception" or "cleanup-failure") throw new InvalidOperationException("فشل اختباري في تحديث العقد");
            if (outcome == "revoked") throw new BranchScopeAccessException();
            if (outcome == "already-absent") File.Delete(newFile);
            return outcome == "success" ? ApiResponse<bool>.Success(true) : ApiResponse<bool>.Fail("تعذر تحديث العقد");
        });
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), "contract.pdf", "contracts", null, null))
            .Returns(async (Stream input, string name, string destination, string[]? extensions, long? size) =>
            {
                sequence.Add("save");
                await using var output = File.Create(newFile);
                await input.CopyToAsync(output);
                return relativePath;
            });
        storage.Setup(s => s.DeleteFileAsync(relativePath)).ReturnsAsync(() =>
        {
            sequence.Add("cleanup");
            if (outcome is "cleanup-failure" or "result-cleanup-failure") throw new IOException("فشل اختباري في التنظيف");
            var exists = File.Exists(newFile);
            File.Delete(newFile);
            return exists;
        });
        try
        {
            await using var app = await MockServiceApp(service.Object, storage.Object);
            using var client = InvestmentProjectHttpHost.Client(app, 7);
            using var content = ContractFile();
            using var response = await client.PostAsync("/api/investment/leasing/contracts/1/upload-signed-contract", content);
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal(newFileRemains, File.Exists(newFile));
            Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(oldFile));
            Assert.Equal(outcome == "success" ? new[] { "authorize", "save", "update" }
                : new[] { "authorize", "save", "update", "cleanup" }, sequence);
            storage.Verify(s => s.DeleteFileAsync(relativePath), outcome == "success" ? Times.Never() : Times.Once());
            storage.Verify(s => s.DeleteFileAsync("uploads/contracts/old.pdf"), Times.Never);
            if (outcome == "success") Assert.True((await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!.Data);
        }
        finally
        {
            File.Delete(newFile);
            File.Delete(oldFile);
            Directory.Delete(folder);
        }
    }

    [Fact]
    public async Task UploadSignedContract_StorageFailureDoesNotUpdateDatabaseOrReportSuccess()
    {
        var service = new Mock<IInvestmentLeasingService>(MockBehavior.Strict);
        service.Setup(s => s.GetLeaseContractByIdAsync(1)).ReturnsAsync(ApiResponse<LeaseContractDto>.Success(new LeaseContractDto { ContractID = 1 }));
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), "contract.pdf", "contracts", null, null))
            .ThrowsAsync(new IOException("فشل اختباري في التخزين"));
        await using var app = await MockServiceApp(service.Object, storage.Object);
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var content = ContractFile();
        using var response = await client.PostAsync("/api/investment/leasing/contracts/1/upload-signed-contract", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        service.Verify(s => s.UpdateLeaseContractFilePathAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        storage.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    internal static MultipartFormDataContent ContractFile()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([37, 80, 68, 70]), "file", "contract.pdf");
        return content;
    }

    private static Mock<IScopedPermissionResolver> Permissions(bool allowed = true)
    {
        var permissions = new Mock<IScopedPermissionResolver>(MockBehavior.Strict);
        permissions.Setup(p => p.HasPermissionAsync(7, It.IsAny<string>())).ReturnsAsync(allowed);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, It.IsAny<string>())).ReturnsAsync(PermissionScope.Restricted([1]));
        return permissions;
    }

    private static Task<WebApplication> MockServiceApp(IInvestmentLeasingService service, IFileStorageService storage) =>
        InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(Permissions().Object);
            services.AddSingleton(service);
            services.AddSingleton(storage);
        });
}
