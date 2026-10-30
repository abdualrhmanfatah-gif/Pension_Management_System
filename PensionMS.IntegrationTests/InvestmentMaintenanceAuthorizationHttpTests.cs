using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Services.Investment;

namespace PensionMS.IntegrationTests;

public class InvestmentMaintenanceAuthorizationHttpTests
{
    private const string Root = "/api/investment/maintenance";

    [Theory]
    [InlineData(false, true, HttpStatusCode.Unauthorized)]
    [InlineData(true, false, HttpStatusCode.Forbidden)]
    [InlineData(true, true, HttpStatusCode.Forbidden)]
    public async Task RequestCreation_RejectsAuthenticationPermissionOrRevokedScopeBeforeRepositoryAccess(
        bool authenticated, bool permitted, HttpStatusCode expected)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var permissions = Permissions(permitted);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "Investment.Manage")).ReturnsAsync(PermissionScope.Denied);
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(unit.Object);
            services.AddSingleton(permissions.Object);
            services.AddScoped<IInvestmentMaintenanceService, InvestmentMaintenanceService>();
        });
        using var client = InvestmentProjectHttpHost.Client(app, authenticated ? 7 : null);
        using var response = await client.PostAsJsonAsync(Root + "/requests?userId=999", new CreateMaintenanceRequestDto
            { PropertyID = 2, Description = "بيانات اختبار", ReportedDate = new DateTime(2026, 1, 1) });
        Assert.Equal(expected, response.StatusCode);
        unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("request-read")]
    [InlineData("request-update")]
    [InlineData("order-create")]
    [InlineData("order-read")]
    [InlineData("order-update")]
    public async Task HiddenEntityFailures_PreserveNotFoundApiResponse(string operation)
    {
        // HTTP/DTO contract only; actual query authorization is tested in unit and real SQL tests.
        var service = new Mock<IInvestmentMaintenanceService>(MockBehavior.Strict);
        service.Setup(s => s.GetMaintenanceRequestByIdAsync(2)).ReturnsAsync(ApiResponse<MaintenanceRequestDto>.Fail("طلب الصيانة غير موجود"));
        service.Setup(s => s.UpdateMaintenanceRequestAsync(2, It.IsAny<UpdateMaintenanceRequestDto>(), 7)).ReturnsAsync(ApiResponse<bool>.Fail("طلب الصيانة غير موجود"));
        service.Setup(s => s.CreateWorkOrderAsync(It.IsAny<CreateWorkOrderDto>(), 7)).ReturnsAsync(ApiResponse<int>.Fail("طلب الصيانة غير موجود"));
        service.Setup(s => s.GetWorkOrderByIdAsync(2)).ReturnsAsync(ApiResponse<WorkOrderDto>.Fail("أمر العمل غير موجود"));
        service.Setup(s => s.UpdateWorkOrderAsync(2, It.IsAny<UpdateWorkOrderDto>(), 7)).ReturnsAsync(ApiResponse<bool>.Fail("أمر العمل غير موجود"));
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(Permissions(true).Object);
            services.AddSingleton(service.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = operation switch
        {
            "request-read" => await client.GetAsync(Root + "/requests/2"),
            "request-update" => await client.PutAsJsonAsync(Root + "/requests/2", new UpdateMaintenanceRequestDto { Status = "Completed" }),
            "order-create" => await client.PostAsJsonAsync(Root + "/work-orders", new CreateWorkOrderDto { RequestID = 2 }),
            "order-read" => await client.GetAsync(Root + "/work-orders/2"),
            _ => await client.PutAsJsonAsync(Root + "/work-orders/2", new UpdateWorkOrderDto())
        };
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(body!.IsSuccess);
        // ApiResponse<T>.Fail preserves default(T): reference reads null, creates 0, updates false.
        if (operation == "order-create") Assert.Equal(0, Assert.IsType<JsonElement>(body.Data).GetInt32());
        else if (operation.EndsWith("update")) Assert.False(Assert.IsType<JsonElement>(body.Data).GetBoolean());
        else Assert.Null(body.Data);
        Assert.Contains("غير موجود", body.Message);
    }

    [Theory]
    [InlineData("properties/2/requests")]
    [InlineData("requests/all")]
    [InlineData("requests/2/work-orders")]
    public async Task ScopedEmptyLists_PreserveSuccessfulResponse(string path)
    {
        var service = new Mock<IInvestmentMaintenanceService>(MockBehavior.Strict);
        service.Setup(s => s.GetRequestsByPropertyAsync(2)).ReturnsAsync(ApiResponse<List<MaintenanceRequestDto>>.Success([]));
        service.Setup(s => s.GetAllMaintenanceRequestsAsync()).ReturnsAsync(ApiResponse<List<MaintenanceRequestDto>>.Success([]));
        service.Setup(s => s.GetWorkOrdersByRequestAsync(2)).ReturnsAsync(ApiResponse<List<WorkOrderDto>>.Success([]));
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(Permissions(true).Object);
            services.AddSingleton(service.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = await client.GetAsync(Root + "/" + path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<object>>>();
        Assert.True(body!.IsSuccess);
        Assert.Empty(body.Data!);
    }

    [Theory]
    [InlineData("validation", HttpStatusCode.BadRequest)]
    [InlineData("database", HttpStatusCode.Conflict)]
    [InlineData("failure", HttpStatusCode.InternalServerError)]
    public async Task Failures_PreserveExistingStatusMapping(string failure, HttpStatusCode expected)
    {
        var service = new Mock<IInvestmentMaintenanceService>(MockBehavior.Strict);
        if (failure == "validation") service.Setup(s => s.CreateMaintenanceRequestAsync(It.IsAny<CreateMaintenanceRequestDto>(), 7))
            .ReturnsAsync(ApiResponse<int>.Fail("بيانات اختبار"));
        else service.Setup(s => s.CreateMaintenanceRequestAsync(It.IsAny<CreateMaintenanceRequestDto>(), 7))
            .ThrowsAsync(failure == "database" ? new DbUpdateException("synthetic-conflict") : new Exception("synthetic-failure"));
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(Permissions(true).Object);
            services.AddSingleton(service.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = await client.PostAsJsonAsync(Root + "/requests", new CreateMaintenanceRequestDto { Description = "بيانات اختبار" });
        Assert.Equal(expected, response.StatusCode);
        Assert.False((await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.IsSuccess);
    }

    private static Mock<IScopedPermissionResolver> Permissions(bool permitted)
    {
        var permissions = new Mock<IScopedPermissionResolver>(MockBehavior.Strict);
        permissions.Setup(p => p.HasPermissionAsync(7, "Investment.View")).ReturnsAsync(permitted);
        permissions.Setup(p => p.HasPermissionAsync(7, "Investment.Manage")).ReturnsAsync(permitted);
        return permissions;
    }
}
