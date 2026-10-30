using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PensionMS.API.Controllers;
using PensionMS.API.Middleware;
using PensionMS.API.Security;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class InvestmentProjectAuthorizationHttpTests
{
    [Theory]
    [InlineData(false, true, HttpStatusCode.Unauthorized)]
    [InlineData(true, false, HttpStatusCode.Forbidden)]
    [InlineData(true, true, HttpStatusCode.Forbidden)]
    public async Task Create_RejectsUnauthenticatedMissingPermissionAndUnauthorizedBranchBeforeDataAccess(
        bool authenticated, bool hasPermission, HttpStatusCode expected)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var permissions = new Mock<IScopedPermissionResolver>(MockBehavior.Strict);
        permissions.Setup(p => p.HasPermissionAsync(7, "Investment.Manage")).ReturnsAsync(hasPermission);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "Investment.Manage"))
            .ReturnsAsync(PermissionScope.Restricted([1]));
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(unit.Object);
            services.AddSingleton(permissions.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, authenticated ? 7 : null);
        using var result = await client.PostAsJsonAsync("/api/investment/projects?userId=999", new CreateProjectDto
        {
            Name = "اختبار منع الإنشاء", PortfolioID = 1, TypeID = 1, PensionBranchID = 2,
            StartDate = DateTime.UtcNow
        });
        Assert.Equal(expected, result.StatusCode);
        if (authenticated && hasPermission)
        {
            var body = await result.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.False(body!.IsSuccess);
            Assert.Null(body.Data);
            Assert.False(string.IsNullOrWhiteSpace(body.Message));
            permissions.Verify(p => p.GetAllowedScopeAsync(7, "Investment.Manage"), Times.Once);
        }
        unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HiddenProject_UsesExistingNotFoundApiResponse()
    {
        // Controller/HTTP contract check. The SQL test below exercises actual scoped lookups.
        var service = new Mock<IInvestmentProjectService>(MockBehavior.Strict);
        service.Setup(s => s.GetProjectByIdAsync(2)).ReturnsAsync(ApiResponse<ProjectDto>.Fail("المشروع غير موجود"));
        var permissions = new Mock<IScopedPermissionResolver>();
        permissions.Setup(p => p.HasPermissionAsync(7, "Investment.View")).ReturnsAsync(true);
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IUnitOfWork>());
            services.AddSingleton(permissions.Object);
            services.AddSingleton(service.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, 7);
        using var response = await client.GetAsync("/api/investment/projects/2");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>();
        Assert.False(body!.IsSuccess);
        Assert.Null(body.Data);
        Assert.Equal("المشروع غير موجود", body.Message);
    }
}

public class InvestmentProjectAuthorizationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task HttpAndSql_EnforceProjectAndPlanScope_AndRejectedWritesLeaveDatabaseUnchanged()
    {
        int actorId, roleId, branchA, branchB, portfolioA, portfolioB, typeId, projectA, projectB, planB, fiscalId;
        var suffix = Guid.NewGuid().ToString("N");
        await using (var db = database.OpenContext())
        {
            var a = new PensionFundBranch { BranchName = "فرع اختبار أ " + suffix };
            var b = new PensionFundBranch { BranchName = "فرع اختبار ب " + suffix };
            var role = new Role { RoleName = "مشاريع " + suffix };
            foreach (var code in new[] { "Investment.View", "Investment.Manage" })
                role.RolePermissions.Add(new RolePermission { Permission = await db.Permissions.SingleAsync(p => p.Code == code) });
            var actor = new User { Username = "project-scope-" + suffix, FullName = "مستخدم اختبار",
                PasswordHash = "synthetic-test-only", Role = role, Branch = a };
            var pa = new InvestmentPortfolio { Name = "محفظة أ " + suffix, Manager = actor };
            var pb = new InvestmentPortfolio { Name = "محفظة ب " + suffix, Manager = actor };
            var type = new InvestmentType { TypeName = "نوع اختبار " + suffix };
            var first = new InvestmentProject { Name = "مشروع أ " + suffix, Portfolio = pa, InvestmentType = type,
                PensionBranch = a, PlannedBudget = 100, StartDate = DateTime.UtcNow };
            var second = new InvestmentProject { Name = "مشروع ب " + suffix, Portfolio = pb, InvestmentType = type,
                PensionBranch = b, PlannedBudget = 200, StartDate = DateTime.UtcNow };
            var fiscal = new FiscalYear { YearName = "سنة اختبار " + suffix,
                StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(11) };
            var plan = new InvestmentPlan { Project = second, FiscalYear = fiscal, PlannedAmount = 20 };
            var propertyType = new PropertyType { TypeName = "عقار اختبار " + suffix };
            db.AddRange(first, second, plan,
                new UserRoleAssignment { User = actor, Role = role, Branch = a },
                new InvestmentProperty { PropertyName = "عقار أ", Project = first, PensionBranch = a,
                    PropertyType = propertyType, TotalArea = 10, InitialValue = 10, AcquisitionDate = DateTime.UtcNow },
                new InvestmentProperty { PropertyName = "عقار ب", Project = first, PensionBranch = b,
                    PropertyType = propertyType, TotalArea = 10, InitialValue = 10, AcquisitionDate = DateTime.UtcNow });
            await db.SaveChangesAsync();
            (actorId, roleId, branchA, branchB, portfolioA, portfolioB, typeId, projectA, projectB, planB, fiscalId) =
                (actor.UserID, role.RoleID, a.PensionBranchID, b.PensionBranchID, pa.PortfolioID, pb.PortfolioID,
                    type.TypeID, first.ProjectID, second.ProjectID, plan.PlanID, fiscal.FiscalYearID);
        }
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddScoped(_ => database.OpenContext());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
        });
        using var client = InvestmentProjectHttpHost.Client(app, actorId);
        const string root = "/api/investment/projects";
        var before = await SnapshotAsync();
        using (var response = await client.GetAsync($"{root}/{projectB}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.GetAsync($"{root}/summary/{projectB}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var listed = await client.GetFromJsonAsync<ApiResponse<List<ProjectDto>>>($"{root}/all");
        Assert.Equal(projectA, Assert.Single(listed!.Data!).ProjectID);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<ProjectDto>>>($"{root}/portfolio/{portfolioB}"))!.Data!);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<InvestmentPlanDto>>>($"{root}/{projectB}/plans"))!.Data!);
        var summary = await client.GetFromJsonAsync<ApiResponse<ProjectSummaryDto>>($"{root}/summary/{projectA}");
        Assert.Equal(1, summary!.Data!.PropertyCount);

        using (var response = await client.PostAsJsonAsync(root, Project(branchB, portfolioA)))
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{projectA}", Project(branchB, portfolioA)))
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{projectB}", Project(branchA, portfolioA)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/{projectB}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync(root, Project(branchA, portfolioB)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{projectA}", Project(branchA, portfolioB)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/plans", new CreateInvestmentPlanDto
            { ProjectID = projectB, FiscalYearID = fiscalId, PlannedAmount = 99 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/plans/{planB}", new UpdateInvestmentPlanDto
            { ProjectID = projectA, FiscalYearID = fiscalId, PlannedAmount = 99 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/plans/{planB}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());

        int createdId;
        using (var response = await client.PostAsJsonAsync(root, Project(branchA, portfolioA)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            createdId = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        int createdPlanId;
        using (var response = await client.PostAsJsonAsync($"{root}/plans", new CreateInvestmentPlanDto
            { ProjectID = createdId, FiscalYearID = fiscalId, PlannedAmount = 50 }))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            createdPlanId = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        using (var response = await client.PutAsJsonAsync($"{root}/plans/{createdPlanId}", new UpdateInvestmentPlanDto
            { ProjectID = projectB, FiscalYearID = fiscalId, PlannedAmount = 60 }))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(createdId, Assert.Single((await client.GetFromJsonAsync<ApiResponse<List<InvestmentPlanDto>>>
            ($"{root}/{createdId}/plans"))!.Data!).ProjectID);
        using (var response = await client.DeleteAsync($"{root}/plans/{createdPlanId}"))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Expand the actual database permission scope, then allow reassignment to that branch.
        await using (var db = database.OpenContext())
        {
            db.UserRoleAssignments.Add(new UserRoleAssignment { UserId = actorId, RoleId = roleId, BranchId = branchB });
            await db.SaveChangesAsync();
        }
        using (var response = await client.PutAsJsonAsync($"{root}/{createdId}", Project(branchB, portfolioA)))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using (var db = database.OpenContext())
            Assert.Equal(branchB, (await db.InvestmentProjects.SingleAsync(p => p.ProjectID == createdId)).PensionBranchID);
        using (var response = await client.DeleteAsync($"{root}/{createdId}"))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using (var db = database.OpenContext())
        {
            var deleted = await db.InvestmentProjects.IgnoreQueryFilters().SingleAsync(p => p.ProjectID == createdId);
            Assert.True(deleted.IsDeleted);
            Assert.Equal(actorId, deleted.DeletedByUserID);
            Assert.True((await db.InvestmentPlans.IgnoreQueryFilters().SingleAsync(p => p.PlanID == createdPlanId)).IsDeleted);
        }

        CreateProjectDto Project(int branch, int portfolio) => new()
        { Name = "مشروع طلب " + suffix, PortfolioID = portfolio, TypeID = typeId, PensionBranchID = branch,
            PlannedBudget = 100, StartDate = DateTime.UtcNow };

        async Task<string> SnapshotAsync()
        {
            await using var db = database.OpenContext();
            return JsonSerializer.Serialize(new
            {
                Projects = await db.InvestmentProjects.IgnoreQueryFilters().OrderBy(p => p.ProjectID)
                    .Select(p => new { p.ProjectID, p.PortfolioID, p.PensionBranchID, p.Name, p.PlannedBudget, p.IsDeleted, p.UpdatedAt }).ToListAsync(),
                Plans = await db.InvestmentPlans.IgnoreQueryFilters().OrderBy(p => p.PlanID)
                    .Select(p => new { p.PlanID, p.ProjectID, p.FiscalYearID, p.PlannedAmount, p.ActualSpent, p.IsDeleted, p.UpdatedAt }).ToListAsync(),
                AuditCount = await db.AuditLogs.CountAsync()
            });
        }
    }
}

internal static class InvestmentProjectHttpHost
{
    private const string TestKey = "InvestmentProjectScopeTests_SyntheticSigningKey_Only";

    public static async Task<WebApplication> StartAsync(Action<IServiceCollection> configure)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        builder.Logging.ClearProviders();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        builder.Services.AddScoped<IAccessiblePensionRepository, AccessiblePensionRepository>();
        builder.Services.AddScoped<IInvestmentProjectService, InvestmentProjectService>();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        configure(builder.Services);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = "project-tests", ValidateAudience = true, ValidAudience = "project-tests",
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero
            });
        builder.Services.AddAuthorization(options =>
        {
            foreach (var code in new[] { "Investment.View", "Investment.Manage" })
                options.AddPolicy(code, p => p.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(code)));
        });
        builder.Services.AddControllers().AddApplicationPart(typeof(InvestmentProjectsController).Assembly);
        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    public static HttpClient Client(WebApplication app, int? userId)
    {
        var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        if (userId.HasValue)
        {
            // A claimed HQ role/branch must never widen the resolver's actual permission scope.
            var token = new JwtSecurityToken("project-tests", "project-tests",
                [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()), new Claim(ClaimTypes.Role, "HQ"), new Claim("BranchId", "2")],
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)), SecurityAlgorithms.HmacSha256));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        }
        return client;
    }
}
