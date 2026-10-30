using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PensionMS.API.Security;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Services.Investment;

namespace PensionMS.IntegrationTests;

public class InvestmentPropertyAuthorizationHttpTests
{
    [Theory]
    [InlineData(false, true, HttpStatusCode.Unauthorized)]
    [InlineData(true, false, HttpStatusCode.Forbidden)]
    [InlineData(true, true, HttpStatusCode.Forbidden)]
    public async Task Create_UsesActualServiceAndAuthenticatedOperationScopeBeforeAnyWrites(
        bool authenticated, bool allowed, HttpStatusCode expected)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var types = new Mock<IRepository<PropertyType>>();
        types.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new PropertyType { TypeID = 1 });
        var users = new Mock<IRepository<User>>();
        users.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new User { UserID = 7, BranchID = 1 });
        unit.Setup(u => u.PropertyTypes).Returns(types.Object);
        unit.Setup(u => u.Users).Returns(users.Object);
        var permissions = Permissions(allowed);
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        await using var app = await StartAsync(services =>
        {
            services.AddSingleton(unit.Object);
            services.AddSingleton(permissions.Object);
            services.AddSingleton(storage.Object);
            services.AddScoped<IInvestmentPropertyService, InvestmentPropertyService>();
        });
        using var client = InvestmentProjectHttpHost.Client(app, authenticated ? 7 : null);
        using var response = await client.PostAsJsonAsync("/api/investment/properties?userId=999", new CreatePropertyDto
        { PropertyName = "عقار اختبار", PropertyTypeID = 1, PensionBranchID = 2, TotalArea = 10 });
        Assert.Equal(expected, response.StatusCode);
        if (authenticated && allowed)
        {
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.False(body!.IsSuccess);
            Assert.Null(body.Data);
            permissions.Verify(p => p.GetAllowedScopeAsync(7, "Investment.Add"), Times.Once);
        }
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        storage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HiddenProperty_PreservesNotFoundResponse()
    {
        var service = new Mock<IInvestmentPropertyService>(MockBehavior.Strict);
        service.Setup(s => s.GetPropertyByIdAsync(2)).ReturnsAsync(ApiResponse<PropertyDetailDto>.Fail("العقار غير موجود"));
        await using var app = await MockServiceApp(service.Object, Mock.Of<IFileStorageService>());
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = await client.GetAsync("/api/investment/properties/2");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PropertyDetailDto>>();
        Assert.False(body!.IsSuccess);
        Assert.Null(body.Data);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task UploadPhoto_RejectsBeforeFileStorageWhenPreflightFails(bool forbidden, HttpStatusCode expected)
    {
        var service = new Mock<IInvestmentPropertyService>(MockBehavior.Strict);
        if (forbidden) service.Setup(s => s.GetPropertyByIdAsync(2)).ThrowsAsync(new BranchScopeAccessException());
        else service.Setup(s => s.GetPropertyByIdAsync(2)).ReturnsAsync(ApiResponse<PropertyDetailDto>.Fail("العقار غير موجود"));
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        await using var app = await MockServiceApp(service.Object, storage.Object);
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var content = Photo();
        using var response = await client.PostAsync("/api/investment/properties/2/upload-photo", content);
        Assert.Equal(expected, response.StatusCode);
        if (!forbidden)
        {
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.False(body!.IsSuccess);
            Assert.False(body.Data);
        }
        service.Verify(s => s.UpdatePropertyMainPhotoPathAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        storage.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("success", HttpStatusCode.OK, true)]
    [InlineData("result-failure", HttpStatusCode.BadRequest, false)]
    [InlineData("exception", HttpStatusCode.BadRequest, false)]
    [InlineData("revoked", HttpStatusCode.Forbidden, false)]
    [InlineData("cleanup-failure", HttpStatusCode.InternalServerError, true)]
    [InlineData("already-absent", HttpStatusCode.BadRequest, false)]
    public async Task UploadPhoto_OrdersAuthorizationBeforeStorage_AndCleansOnlyTheNewFile(
        string outcome, HttpStatusCode expected, bool newFileRemains)
    {
        var folder = Path.Combine(Path.GetTempPath(), "PensionMS_PropertyPhoto_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var newFile = Path.Combine(folder, "new.png");
        var oldFile = Path.Combine(folder, "old.png");
        await File.WriteAllBytesAsync(oldFile, [1, 2, 3]);
        const string relativePath = "uploads/properties/new.png";
        var sequence = new List<string>();
        var service = new Mock<IInvestmentPropertyService>(MockBehavior.Strict);
        service.Setup(s => s.GetPropertyByIdAsync(1)).Callback(() => sequence.Add("authorize"))
            .ReturnsAsync(ApiResponse<PropertyDetailDto>.Success(new PropertyDetailDto
            { PropertyID = 1, MainPhotoPath = "uploads/properties/old.png" }));
        service.Setup(s => s.UpdatePropertyMainPhotoPathAsync(1, relativePath, 7)).ReturnsAsync(() =>
        {
            sequence.Add("update");
            if (outcome is "exception" or "cleanup-failure") throw new InvalidOperationException("فشل اختباري في تحديث الصورة");
            if (outcome == "revoked") throw new BranchScopeAccessException();
            if (outcome == "already-absent") File.Delete(newFile);
            return outcome == "success" ? ApiResponse<bool>.Success(true) : ApiResponse<bool>.Fail("تعذر تحديث الصورة");
        });
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), "photo.png", "properties", null, null))
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
            if (outcome == "cleanup-failure") throw new IOException("فشل اختباري في التنظيف");
            var exists = File.Exists(newFile);
            File.Delete(newFile);
            return exists;
        });
        try
        {
            await using var app = await MockServiceApp(service.Object, storage.Object);
            using var client = InvestmentProjectHttpHost.Client(app, 7);
            using var content = Photo();
            using var response = await client.PostAsync("/api/investment/properties/1/upload-photo", content);
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal(newFileRemains, File.Exists(newFile));
            Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(oldFile));
            Assert.Equal(outcome == "success" ? new[] { "authorize", "save", "update" }
                : new[] { "authorize", "save", "update", "cleanup" }, sequence);
            storage.Verify(s => s.DeleteFileAsync(relativePath), outcome == "success" ? Times.Never() : Times.Once());
            storage.Verify(s => s.DeleteFileAsync("uploads/properties/old.png"), Times.Never);
            if (outcome == "success")
                Assert.True((await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!.Data);
        }
        finally
        {
            File.Delete(newFile);
            File.Delete(oldFile);
            Directory.Delete(folder);
        }
    }

    internal static Task<WebApplication> StartAsync(Action<IServiceCollection> configure) =>
        InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddAuthorization(options =>
            {
                foreach (var code in new[] { "Investment.Add", "Investment.Edit", "Investment.Delete" })
                    options.AddPolicy(code, p => p.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(code)));
            });
            configure(services);
        });

    internal static MultipartFormDataContent Photo()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([137, 80, 78, 71]), "file", "photo.png");
        return content;
    }

    private static Mock<IScopedPermissionResolver> Permissions(bool allowed = true)
    {
        var permissions = new Mock<IScopedPermissionResolver>(MockBehavior.Strict);
        permissions.Setup(p => p.HasPermissionAsync(7, It.IsAny<string>())).ReturnsAsync(allowed);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, It.IsAny<string>())).ReturnsAsync(PermissionScope.Restricted([1]));
        return permissions;
    }

    private static Task<WebApplication> MockServiceApp(IInvestmentPropertyService service, IFileStorageService storage) =>
        StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(Permissions().Object);
            services.AddSingleton(service);
            services.AddSingleton(storage);
        });
}
