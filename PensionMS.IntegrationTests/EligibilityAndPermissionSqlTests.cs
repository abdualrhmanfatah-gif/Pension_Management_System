using PensionMS.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Reporting;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class EligibilityAndPermissionSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task Eligibility_DeactivatesLinkedRowsInSameSave_EvenWhenPersonAlreadyInactive()
    {
        int entitledId, agentId, recipientId;
        await using (var db = database.OpenContext())
        {
            var user = new User { Username = "eligibility", FullName = "اختبار", PasswordHash = "test" };
            var ministry = new MinistryApprovalBatch { ImportedByUser = user, Status = "Test" };
            var file = new RetirementFile { FileNumber = "eligibility", StatusID = FileStatusConstants.Active,
                PensionBranch = new PensionFundBranch { BranchName = "اختبار استحقاق" },
                Retiree = new Retiree { FullName = "اختبار", NationalID = "eligibility", IsAlive = true,
                    ApprovalItem = new MinistryApprovalItem { FullName = "اختبار", Batch = ministry } } };
            var entitled = new EntitledPerson { RetirementFile = file, FullName = "اختبار مستحق", NationalID = "eligibility-person",
                Relationship = new Relationship { RelationshipName = "اختبار" }, BirthDate = new DateOnly(2000, 1, 1),
                IsActive = false, StopCode = EntitledStopCodes.RetireeAlive };
            var agent = new Agent { RetirementFile = file, EntitledPerson = entitled, FullName = "اختبار وكيل", NationalID = "eligibility-agent" };
            var recipient = new SalaryRecipient { RetirementFile = file, EntitledPerson = entitled,
                RecipientType = "Entitled", StartDate = DateTime.UtcNow };
            db.AddRange(agent, recipient);
            await db.SaveChangesAsync();
            (entitledId, agentId, recipientId) = (entitled.EntitledID, agent.AgentID, recipient.RecipientID);
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
            Assert.True((await new EntitlementEngine(uow).ReEvaluateEntitlementStatusAsync(entitledId)).IsSuccess);
        await using (var db = database.OpenContext())
        {
            Assert.False((await db.EntitledPersons.FindAsync(entitledId))!.IsActive);
            Assert.False((await db.Agents.FindAsync(agentId))!.IsActive);
            Assert.False((await db.SalaryRecipients.FindAsync(recipientId))!.IsActive);
        }
    }

    [Fact]
    public async Task PermissionProjection_PreservesIndividualOverridesAndAssignmentFallbackRules()
    {
        await using var db = database.OpenContext();
        var parent = new PensionFundBranch { BranchName = "اختبار إقليم" };
        var child = new PensionFundBranch { BranchName = "اختبار فرع", ParentBranch = parent };
        var other = new PensionFundBranch { BranchName = "اختبار آخر" };
        var permission = new Permission { Code = "Test.Scope" };
        var allowed = new Role { RoleName = "test-allowed" };
        allowed.RolePermissions.Add(new RolePermission { Permission = permission });
        var empty = new Role { RoleName = "test-empty" };
        var user = new User { Username = "permission", FullName = "اختبار", PasswordHash = "test", Role = allowed };
        db.AddRange(child, other, user, empty);
        await db.SaveChangesAsync();
        var resolver = new ScopedPermissionResolver(db);
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, other.PensionBranchID));
        Assert.Empty(await resolver.GetUserPermissionsAsync(user.UserID));
        var assignment = new UserRoleAssignment { User = user, Role = empty, BranchId = child.PensionBranchID };
        db.Add(assignment);
        await db.SaveChangesAsync();
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, child.PensionBranchID));
        assignment.Role = allowed;
        await db.SaveChangesAsync();
        Assert.True(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, child.PensionBranchID));
        Assert.Contains(permission.Code, await resolver.GetUserPermissionsAsync(user.UserID));
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, other.PensionBranchID));
        assignment.BranchId = null;
        assignment.RegionId = parent.PensionBranchID;
        await db.SaveChangesAsync();
        Assert.True(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, child.PensionBranchID));
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, other.PensionBranchID));
        var individual = new UserPermission { User = user, Permission = permission, IsGranted = false };
        db.Add(individual);
        await db.SaveChangesAsync();
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, child.PensionBranchID));
        Assert.DoesNotContain(permission.Code, await resolver.GetUserPermissionsAsync(user.UserID));
        individual.IsGranted = true;
        await db.SaveChangesAsync();
        Assert.False(await resolver.HasPermissionInBranchAsync(user.UserID, permission.Code, other.PensionBranchID));
        Assert.Contains(permission.Code, await resolver.GetUserPermissionsAsync(user.UserID));
    }

    [Fact]
    public async Task KashfSummary_TranslatesGroupingAndReturnsZeroForEmptyFilter()
    {
        await using var db = database.OpenContext();
        var user = new User { Username = "summary", FullName = "اختبار", PasswordHash = "test" };
        var batch = new MinistryApprovalBatch { ImportedByUser = user, Status = "Test" };
        db.AddRange(new MinistryApprovalItem { Batch = batch, FullName = "اختبار أ", MilitaryNumber = "1",
                UnitName = "اختبار وحدة", Gender = 1 },
            new MinistryApprovalItem { Batch = batch, FullName = "اختبار ب", MilitaryNumber = "2",
                UnitName = "اختبار وحدة", Gender = 2, IsProcessed = true });
        await db.SaveChangesAsync();
        var scope = new Mock<ITestBranchScope>();
        scope.Setup(s => s.IsHQ()).Returns(true);
        using var uow = new UnitOfWork(db);
        var service = new DiscrepancyService(uow, TestPensionAccess.From(uow, scope.Object),
            new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportExporter(
                new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportTemplate()));
        var summary = (await service.GetKashfReportSummaryAsync(new DiscrepancyFilterDto { UnitName = "اختبار وحدة" })).Data!;
        Assert.Equal(2, summary.TotalCount);
        Assert.Equal(1, summary.ProcessedCount);
        Assert.Equal(2, Assert.Single(summary.ByUnit).Count);
        Assert.Equal(2, summary.ByGender.Count);
        var empty = (await service.GetKashfReportSummaryAsync(new DiscrepancyFilterDto { UnitName = "غير موجود" })).Data!;
        Assert.Equal(0, empty.TotalCount);
        Assert.Empty(empty.ByUnit);
        Assert.Empty(empty.ByGender);
    }
}
