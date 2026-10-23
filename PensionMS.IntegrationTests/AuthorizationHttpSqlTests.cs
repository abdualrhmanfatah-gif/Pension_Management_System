using PensionMS.Testing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PensionMS.API.Controllers;
using PensionMS.API.Middleware;
using PensionMS.API.Security;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Core.Reporting;
using PensionMS.Infrastructure.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Reporting;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class AuthorizationHttpSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    private const string Key = "PensionMS-test-only-signing-key-0123456789";

    [Fact]
    public async Task HttpPipeline_EnforcesBranchScopeAndRejectsDeletedUsersOldToken()
    {
        int branchA, branchB, userA, userB, hqId, missingBranchId, deniedId;
        await using (var db = database.OpenContext())
        {
            var a = new PensionFundBranch { BranchName = "اختبار أ" };
            var b = new PensionFundBranch { BranchName = "اختبار ب" };
            var role = new Role { RoleName = "HTTP-test" };
            foreach (var code in new[] { "Dashboard.View", "Report.Pension.View" })
                role.RolePermissions.Add(new RolePermission { Permission = new Permission { Code = code } });
            User Make(string name, PensionFundBranch? branch, bool allowed = true) => new()
            { Username = name, FullName = name, PasswordHash = "test", Branch = branch, Role = allowed ? role : null };
            var ua = Make("http-a", a);
            var ub = Make("http-b", b);
            var hq = Make("http-hq", null);
            var missing = Make("http-missing", null);
            var denied = Make("http-denied", a, false);
            db.AddRange(ua, ub, hq, missing, denied);
            var ministry = new MinistryApprovalBatch { ImportedByUser = hq, Status = "Test" };
            db.Add(new RetirementFile { FileNumber = "http-file-a", PensionBranch = a,
                Retiree = new Retiree { FullName = "متقاعد أ", NationalID = "http-a",
                    ApprovalItem = new MinistryApprovalItem { Batch = ministry, FullName = "متقاعد أ", MilitaryNumber = "101" } }, StatusID = FileStatusConstants.Active });
            db.Add(new RetirementFile { FileNumber = "http-file-b", PensionBranch = b,
                Retiree = new Retiree { FullName = "متقاعد ب", NationalID = "http-b",
                    ApprovalItem = new MinistryApprovalItem { Batch = ministry, FullName = "متقاعد ب", MilitaryNumber = "102" } }, StatusID = FileStatusConstants.Active });
            await db.SaveChangesAsync();
            db.UserRoleAssignments.AddRange(
                new UserRoleAssignment { User = ua, Role = role, BranchId = a.PensionBranchID },
                new UserRoleAssignment { User = ub, Role = role, BranchId = b.PensionBranchID },
                new UserRoleAssignment { User = hq, Role = role });
            await db.SaveChangesAsync();
            (branchA, branchB, userA, userB, hqId, missingBranchId, deniedId) =
                (a.PensionBranchID, b.PensionBranchID, ua.UserID, ub.UserID, hq.UserID, missing.UserID, denied.UserID);
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        builder.Services.AddScoped(_ => database.OpenContext());
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IAccessiblePensionRepository, AccessiblePensionRepository>();
        builder.Services.AddSingleton(Mock.Of<INotificationService>());
        builder.Services.AddSingleton(Mock.Of<IImmutableAuditService>());
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddSingleton(Mock.Of<IMissingDocumentService>());
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IReportDataService, ReportDataService>();
        builder.Services.AddSingleton<IReportRegistry, ReportRegistry>();
        builder.Services.AddSingleton<ExcelReportTemplate>();
        builder.Services.AddSingleton<ExcelReportExporter>();
        builder.Services.AddSingleton<PdfReportExporter>();
        builder.Services.AddControllers().AddApplicationPart(typeof(DashboardController).Assembly);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = "test", ValidateAudience = true, ValidAudience = "test",
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents { OnTokenValidated = ActiveUserValidation.ValidateAsync };
        });
        builder.Services.AddAuthorization(options =>
        {
            foreach (var code in new[] { "Dashboard.View", "Report.Pension.View" })
                options.AddPolicy(code, p => p.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(code)));
        });
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };

        string Token(int user, int? branch, bool hq = false) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            "test", "test", new[] { new Claim(ClaimTypes.NameIdentifier, user.ToString()),
                new Claim("BranchId", branch?.ToString() ?? ""), new Claim("IsHQ", hq.ToString()) },
            expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256)));
        async Task<HttpResponseMessage> Get(string token, string path)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await client.SendAsync(request);
        }
        async Task<int> Count(string token, string path, string property)
        {
            using var response = await Get(token, path);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{path}: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("data").GetProperty(property).GetInt32();
        }
        var tokenA = Token(userA, branchA);
        Assert.Equal(1, await Count(tokenA, "/api/Dashboard/stats", "totalRetirees"));
        Assert.Equal(1, await Count(Token(userB, branchB), "/api/Dashboard/stats", "totalRetirees"));
        Assert.Equal(2, await Count(Token(hqId, null, true), "/api/Dashboard/stats", "totalRetirees"));
        Assert.Equal(1, await Count(tokenA, "/api/Reports/retirement/file-status", "totalCount"));
        Assert.Equal(1, await Count(Token(hqId, null, true), $"/api/Reports/retirement/file-status?BranchId={branchB}", "totalCount"));
        foreach (var suffix in new[] { "", "/export?format=excel&", "/export?format=pdf&" })
        {
            var path = suffix.Length == 0 ? $"?BranchId={branchB}" : $"{suffix}BranchId={branchB}";
            using var response = await Get(tokenA, "/api/Reports/retirement/file-status" + path);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        using (var response = await Get(Token(missingBranchId, null), "/api/Dashboard/stats"))
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await Get(Token(deniedId, branchA), "/api/Dashboard/stats"))
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await Get("invalid-token", "/api/Dashboard/stats"))
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
            Assert.True(await new UserService(uow, Mock.Of<INotificationService>(), PensionMS.Testing.TestPensionAccess.All(uow), Mock.Of<IImmutableAuditService>(), new ScopedPermissionResolver(db)).DeleteUserAsync(userA, hqId));
        using (var response = await Get(tokenA, "/api/Dashboard/stats"))
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await app.StopAsync();
    }
}
