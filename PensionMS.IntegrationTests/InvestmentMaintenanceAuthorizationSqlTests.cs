using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class InvestmentMaintenanceAuthorizationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task HttpAndSql_EnforceMaintenanceOwnership_RejectWrites_AndPreserveLifecycle()
    {
        int actorId, roleId, branchB, propertyA, propertyB, unitA, unitB, wrongUnitA,
            tenantA, tenantB, sharedTenant, unlinkedTenant, requestA, requestB, orderA, orderB, legacyRequest;
        var suffix = Guid.NewGuid().ToString("N");
        await using (var db = database.OpenContext())
        {
            var a = new PensionFundBranch { BranchName = "صيانة أ " + suffix };
            var b = new PensionFundBranch { BranchName = "صيانة ب " + suffix };
            var role = new Role { RoleName = "بيانات اختبار" + suffix };
            foreach (var code in new[] { "Investment.View", "Investment.Manage" })
                role.RolePermissions.Add(new RolePermission { Permission = await db.Permissions.SingleAsync(p => p.Code == code) });
            var actor = new User { Username = "maintenance-scope-" + suffix, FullName = "بيانات اختبار",
                PasswordHash = "synthetic-test-only", Role = role, Branch = a };
            var propertyType = new PropertyType { TypeName = "بيانات اختبار" + suffix };
            var unitType = new UnitType { TypeName = "بيانات اختبار" + suffix };
            InvestmentProperty Property(string name, PensionFundBranch branch) => new()
                { PropertyName = name, PensionBranch = branch, PropertyType = propertyType, TotalArea = 100, InitialValue = 1000 };
            var pa = Property("عقار أ", a); var pb = Property("عقار ب", b); var other = Property("عقار آخر أ", a);
            PropertyUnit Unit(InvestmentProperty property, string number) => new()
                { Property = property, UnitType = unitType, UnitNumber = number, Area = 10 };
            var ua = Unit(pa, "a1"); var ub = Unit(pb, "b1"); var wrong = Unit(other, "a-other");
            Tenant Tenant(string name) => new() { FullName = name, NationalIDOrCR = name + suffix };
            var ta = Tenant("مستأجر أ"); var tb = Tenant("مستأجر ب"); var shared = Tenant("مشترك"); var unlinked = Tenant("غير مرتبط");
            LeaseContract Lease(PropertyUnit unit, Tenant tenant) => new() { Unit = unit, Tenant = tenant,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2027, 1, 1), TotalContractValue = 100,
                Status = LeaseStatusConstants.Active, PaymentFrequency = PaymentFrequencyConstants.Monthly };
            var ra = new MaintenanceRequest { Property = pa, Unit = ua, Tenant = shared, Description = "بيانات اختبار", ReportedDate = new DateTime(2026, 1, 1) };
            var rb = new MaintenanceRequest { Property = pb, Unit = ub, Tenant = tb, Description = "بيانات اختبار", ReportedDate = new DateTime(2026, 2, 1) };
            // Existing schema permits this inconsistent optional relationship; reads must mask it.
            var legacy = new MaintenanceRequest { Property = pa, Unit = ub, Tenant = tb, Description = "بيانات اختبار", ReportedDate = new DateTime(2026, 3, 1) };
            var wa = new WorkOrder { MaintenanceRequest = ra, EstimatedCost = 10, InvoiceFilePath = "old.pdf" };
            var wb = new WorkOrder { MaintenanceRequest = rb, EstimatedCost = 20, InvoiceFilePath = "foreign.pdf" };
            db.AddRange(Lease(ua, ta), Lease(ub, tb), Lease(Unit(pa, "a2"), shared), Lease(Unit(pb, "b2"), shared),
                wrong, unlinked, ra, rb, legacy, wa, wb,
                new UserRoleAssignment { User = actor, Role = role, Branch = a });
            await db.SaveChangesAsync();
            (actorId, roleId, branchB, propertyA, propertyB, unitA, unitB, wrongUnitA,
                tenantA, tenantB, sharedTenant, unlinkedTenant, requestA, requestB, orderA, orderB, legacyRequest) =
                (actor.UserID, role.RoleID, b.PensionBranchID, pa.PropertyID, pb.PropertyID, ua.UnitID, ub.UnitID, wrong.UnitID,
                    ta.TenantID, tb.TenantID, shared.TenantID, unlinked.TenantID, ra.RequestID, rb.RequestID, wa.WorkOrderID, wb.WorkOrderID, legacy.RequestID);
        }
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddScoped(_ => database.OpenContext());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
            services.AddScoped<IInvestmentMaintenanceService, InvestmentMaintenanceService>();
        });
        using var client = InvestmentProjectHttpHost.Client(app, actorId);
        const string root = "/api/investment/maintenance";
        var before = await SnapshotAsync();
        using (var response = await client.GetAsync($"{root}/requests/{requestB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.GetAsync($"{root}/work-orders/{orderB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        foreach (var path in new[] { "requests/all", "properties/0/requests", "properties/-1/requests" })
        {
            var rows = (await client.GetFromJsonAsync<ApiResponse<List<MaintenanceRequestDto>>>($"{root}/{path}"))!.Data!;
            Assert.Equal(new[] { requestA, legacyRequest }.Order(), rows.Select(r => r.RequestID).Order());
            var historical = rows.Single(r => r.RequestID == legacyRequest);
            Assert.Null(historical.UnitID); Assert.Null(historical.UnitNumber);
            Assert.Null(historical.TenantID); Assert.Null(historical.TenantName);
        }
        Assert.Equal("مشترك", (await client.GetFromJsonAsync<ApiResponse<MaintenanceRequestDto>>($"{root}/requests/{requestA}"))!.Data!.TenantName);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<MaintenanceRequestDto>>>($"{root}/properties/{propertyB}/requests"))!.Data!);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<WorkOrderDto>>>($"{root}/requests/{requestB}/work-orders"))!.Data!);
        Assert.Equal(orderA, Assert.Single((await client.GetFromJsonAsync<ApiResponse<List<WorkOrderDto>>>($"{root}/requests/{requestA}/work-orders"))!.Data!).WorkOrderID);
        foreach (var dto in new[] { Request(propertyB, unitB, tenantB), Request(propertyA, unitB, tenantA),
            Request(propertyA, wrongUnitA, tenantA), Request(propertyA, unitA, tenantB) })
        {
            using var response = await client.PostAsJsonAsync(root + "/requests", dto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        using (var response = await client.PutAsJsonAsync($"{root}/requests/{requestB}", new UpdateMaintenanceRequestDto { Status = "Completed" }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync(root + "/work-orders", new CreateWorkOrderDto { RequestID = requestB, EstimatedCost = 90 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/work-orders/{orderB}", Update()))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());

        int createdRequest, createdOrder;
        using (var response = await client.PostAsJsonAsync(root + "/requests", Request(propertyA, unitA, sharedTenant)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            createdRequest = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        using (var response = await client.PostAsJsonAsync(root + "/requests", Request(propertyA, null, unlinkedTenant)))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/requests/{createdRequest}", new UpdateMaintenanceRequestDto { Status = "New" }))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.PostAsJsonAsync(root + "/work-orders", new CreateWorkOrderDto
            { RequestID = createdRequest, AssignedContractor = "بيانات اختبار", EstimatedCost = 75 }))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            createdOrder = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        await using (var db = database.OpenContext())
            Assert.Equal(MaintenanceStatusConstants.InProgress, (await db.Set<MaintenanceRequest>().SingleAsync(r => r.RequestID == createdRequest)).Status);
        using (var response = await client.PutAsJsonAsync($"{root}/work-orders/{createdOrder}", Update()))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var completed = (await client.GetFromJsonAsync<ApiResponse<WorkOrderDto>>($"{root}/work-orders/{createdOrder}"))!.Data!;
        Assert.Equal(30, completed.ActualCost); Assert.Equal("new.pdf", completed.InvoiceFilePath);
        await using (var db = database.OpenContext())
        {
            Assert.Equal(MaintenanceStatusConstants.Completed, (await db.Set<MaintenanceRequest>().SingleAsync(r => r.RequestID == createdRequest)).Status);
            db.UserRoleAssignments.Add(new UserRoleAssignment { UserId = actorId, RoleId = roleId, BranchId = branchB });
            await db.SaveChangesAsync();
        }
        Assert.True((await client.GetFromJsonAsync<ApiResponse<MaintenanceRequestDto>>($"{root}/requests/{requestB}"))!.IsSuccess);
        using (var response = await client.PostAsJsonAsync(root + "/requests", Request(propertyB, unitB, tenantB)))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/work-orders/{orderB}", Update()))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Even an expanded scope must not permit mismatching the request's property and unit.
        using (var response = await client.PostAsJsonAsync(root + "/requests", Request(propertyA, unitB, sharedTenant)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        CreateMaintenanceRequestDto Request(int property, int? unit, int? tenant) => new()
            { PropertyID = property, UnitID = unit, TenantID = tenant, Description = "بيانات اختبار", ReportedDate = new DateTime(2026, 10, 1) };
        UpdateWorkOrderDto Update() => new() { ActualCost = 30, CompletionDate = new DateTime(2026, 10, 2),
            InvoiceFilePath = "new.pdf", CompletionCertPath = "cert.pdf" };
        async Task<string> SnapshotAsync()
        {
            await using var db = database.OpenContext();
            return JsonSerializer.Serialize(new
            {
                Requests = await db.Set<MaintenanceRequest>().IgnoreQueryFilters().OrderBy(r => r.RequestID)
                    .Select(r => new { r.RequestID, r.PropertyID, r.UnitID, r.TenantID, r.Priority, r.Description,
                        r.DamagePhotosJson, r.Status, r.ReportedDate, r.CreatedAt, r.UpdatedAt, r.IsDeleted,
                        r.DeletedAt, r.DeletedByUserID, r.RowVersion }).ToListAsync(),
                Orders = await db.Set<WorkOrder>().IgnoreQueryFilters().OrderBy(w => w.WorkOrderID)
                    .Select(w => new { w.WorkOrderID, w.RequestID, w.AssignedContractor, w.EstimatedCost, w.ActualCost,
                        w.CompletionDate, w.InvoiceFilePath, w.CompletionCertPath, w.CreatedAt, w.UpdatedAt,
                        w.IsDeleted, w.DeletedAt, w.DeletedByUserID, w.RowVersion }).ToListAsync(),
                AuditCount = await db.AuditLogs.CountAsync()
            });
        }
    }
}
