using PensionMS.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Reporting;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.Tests.Services;

public class BranchAccessDecisionTests
{
    [Fact]
    public void PersonalGrant_CannotExpandLocalWriteThroughAnUnrelatedCentralReadRole()
    {
        ScopeAssignment[] functional = [new(null, null), new(2, null)];
        var write = PermissionScopeRules.ResolvePermission([new(2, null)], functional, [], true, 2);
        Assert.Equal(new[] { 2 }, write.BranchIds);
        var extraOperation = PermissionScopeRules.ResolvePermission([], functional, [], true, 2);
        Assert.Equal(new[] { 2 }, extraOperation.BranchIds);
        Assert.True(PermissionScopeRules.ResolvePermission([], functional, [], true, null).IsDenied);
        Assert.True(PermissionScopeRules.ResolvePermission([new(null, null)], functional, [], false, 2).IsDenied);
    }
    [Fact]
    public void RegionalAssignments_IncludeDescendantsOnce_AndSurviveCycles()
    {
        var scope = PermissionScopeRules.Resolve([new(8, null), new(null, 2)],
            [new(2, 4), new(3, 2), new(4, 3), new(5, null)]);
        Assert.Equal(new[] { 2, 3, 4, 8 }, scope.BranchIds);
        Assert.False(scope.Allows(5));
        Assert.True(PermissionScopeRules.Resolve([new(null, null)], []).AllBranches);
        Assert.True(PermissionScopeRules.Resolve([], []).IsDenied);
        Assert.True(PermissionScope.Restricted([0, -1]).IsDenied);
    }

    [Fact]
    public async Task SameUser_CanReadCentrallyAndEditOnlyTheAssignedBranch()
    {
        var unit = new Mock<IUnitOfWork>();
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new[]
        {
            new RetirementFile { FileID = 1, PensionBranchID = 2 },
            new RetirementFile { FileID = 2, PensionBranchID = 3 },
            new RetirementFile { FileID = 3, PensionBranchID = 2, IsDeleted = true }
        }.AsQueryable());
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var identity = Mock.Of<ICurrentUserContext>(i => i.GetCurrentUserId() == 7);
        var permissions = new Mock<IScopedPermissionResolver>();
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "File.View.Sensitive")).ReturnsAsync(PermissionScope.All);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "File.Edit.BasicData")).ReturnsAsync(PermissionScope.Restricted([2]));
        var repository = new AccessiblePensionRepository(unit.Object, identity, permissions.Object, new HttpContextAccessor());

        var read = await repository.ForPermissionAsync("File.View.Sensitive");
        var edit = await repository.ForPermissionAsync("File.Edit.BasicData");
        Assert.Equal(2, read.Files().Count());
        Assert.Equal(1, Assert.Single(edit.Files()).FileID);
        Assert.Throws<BranchScopeAccessException>(() => edit.Files(3));
        // A later decision observes revoked permission; a previous snapshot does not become a second policy.
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "File.Edit.BasicData")).ReturnsAsync(PermissionScope.Denied);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => repository.ForPermissionAsync("File.Edit.BasicData"));
        permissions.Verify(p => p.GetAllowedScopeAsync(7, "File.Edit.BasicData"), Times.Exactly(2));
    }

    [Fact]
    public async Task CurrentOperation_IntersectsServerPolicies_AndRejectsMissingMetadata()
    {
        var identity = Mock.Of<ICurrentUserContext>(i => i.GetCurrentUserId() == 7);
        var permissions = new Mock<IScopedPermissionResolver>();
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "Read")).ReturnsAsync(PermissionScope.Restricted([2, 3]));
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "Write")).ReturnsAsync(PermissionScope.Restricted([3, 4]));
        var http = new DefaultHttpContext();
        http.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthorizeAttribute("Read"), new AuthorizeAttribute("Write")), "test"));
        var repository = new AccessiblePensionRepository(Mock.Of<IUnitOfWork>(), identity, permissions.Object,
            new HttpContextAccessor { HttpContext = http });
        Assert.Equal(new[] { 3 }, (await repository.ForCurrentOperationAsync()).Scope.BranchIds);
        http.SetEndpoint(null);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => repository.ForCurrentOperationAsync());
    }

    [Fact]
    public void MultiBranchOwnership_TranslatesToSqlBeforePagingAndIncludesIndirectDocuments()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AccessModelOnly;Integrated Security=True").Options);
        using var unit = new UnitOfWork(db);
        var queries = new PensionDataQuery(unit, PermissionScope.Restricted([2, 3]));
        var sql = queries.Files().OrderBy(f => f.FileID).Skip(10).Take(5).ToQueryString();
        Assert.Contains("PensionBranchID", sql);
        Assert.Contains("OFFSET", sql);
        foreach (var source in new[]
        {
            queries.Query(unit.Documents).ToQueryString(),
            queries.Query(unit.GuardianEntitledPersons).ToQueryString(),
            queries.Query(unit.PaymentBooks).ToQueryString(),
            queries.Query(unit.SalaryPaymentBatches).ToQueryString()
        }) Assert.Contains("PensionBranchID", source);
        Assert.Throws<BranchScopeAccessException>(() => queries.Query(unit.Documents, 4));
    }
}
