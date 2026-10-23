using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;

namespace PensionMS.IntegrationTests;

public class PensionOwnershipSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_TracksProfileWithoutSavingAndHonorsOuterCommitOrRollback(bool commit)
    {
        await using var db = database.OpenContext();
        var retiree = new Retiree
        {
            FullName = "قبل التهيئة", NationalID = "opening-" + commit, IsAlive = true,
            ApprovalItem = new MinistryApprovalItem
            {
                FullName = "قبل التهيئة",
                Batch = new MinistryApprovalBatch { ImportedByUser = new User
                    { Username = "opening-" + commit, FullName = "اختبار", PasswordHash = "test" }, Status = "Test" }
            }
        };
        db.Add(retiree);
        await db.SaveChangesAsync();
        var id = retiree.RetireeID;
        var userId = retiree.ApprovalItem.Batch.ImportedByUserID;
        db.ChangeTracker.Clear();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RetireeService(unit, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IMissingDocumentService>(), NullLogger<RetireeService>.Instance,
            cache, Access(unit, PermissionScope.All));
        await unit.BeginTransactionAsync();
        var result = await service.StageFileOpeningAsync(id, new UpdateRetireeDto
        {
            FullName = "بعد التهيئة", BirthDate = new DateOnly(1950, 2, 3),
            HealthStatusID = HealthStatusConstants.Deceased
        }, false, new DateOnly(2024, 3, 4), userId);
        Assert.True(result.IsSuccess, result.Message);
        await using (var beforeSave = database.OpenContext())
            Assert.Equal("قبل التهيئة", (await beforeSave.Retirees.FindAsync(id))!.FullName);
        await unit.SaveChangesAsync();
        if (commit) await unit.CommitAsync();
        else await unit.RollbackAsync();
        await using var verified = database.OpenContext();
        var saved = (await verified.Retirees.FindAsync(id))!;
        Assert.Equal(commit ? "بعد التهيئة" : "قبل التهيئة", saved.FullName);
        Assert.Equal(!commit, saved.IsAlive);
        Assert.Equal(commit ? new DateOnly(2024, 3, 4) : (DateOnly?)null, saved.DeathDate);
        Assert.Equal(commit ? new DateOnly(1950, 2, 3) : (DateOnly?)null, saved.BirthDate);
        Assert.Equal(commit ? userId : (int?)null, saved.UpdatedByUserID);
        Assert.Equal("opening-" + commit, saved.NationalID);
    }

    [Fact]
    public async Task FileOpening_CannotResetLifeAfterSoftDeletionOfPreviousFile()
    {
        await using var db = database.OpenContext();
        var (file, _, actor) = await SeedAsync(db, "opening-deleted");
        file.IsDeleted = true;
        await db.SaveChangesAsync();
        var id = file.RetireeID;
        db.ChangeTracker.Clear();
        Assert.False(await db.RetirementFiles.AnyAsync(f => f.FileID == file.FileID));
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RetireeService(unit, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IMissingDocumentService>(), NullLogger<RetireeService>.Instance,
            cache, Access(unit, PermissionScope.All));
        await unit.BeginTransactionAsync();
        var result = await service.StageFileOpeningAsync(id, new UpdateRetireeDto(), false,
            new DateOnly(2024, 3, 4), actor.UserID);
        Assert.False(result.IsSuccess);
        await unit.RollbackAsync();
        Assert.True((await db.Retirees.FindAsync(id))!.IsAlive);
    }

    [Fact]
    public async Task Rollout_PreviewsByDefault_AndOnlyCreatesReviewedOrLegacyLocalAssignments()
    {
        await using var db = database.OpenContext();
        var role = new Role { RoleName = "rollout-admin" };
        role.RolePermissions.Add(new RolePermission
        {
            Permission = await db.Permissions.SingleAsync(p => p.Code == "System.ManageUsers")
        });
        var branch = new PensionFundBranch { BranchName = "فرع التسوية" };
        User Make(string name, bool local) => new()
        {
            Username = name, FullName = name, PasswordHash = "test", Role = role, Branch = local ? branch : null
        };
        var central = Make("rollout-central", false);
        var legacy = Make("rollout-local", true);
        var revoked = Make("rollout-revoked", true);
        var missing = Make("rollout-missing", false);
        db.AddRange(central, legacy, revoked, missing);
        db.UserRoleAssignments.Add(new UserRoleAssignment { User = revoked, Role = role, Branch = branch, IsActive = false });
        await db.SaveChangesAsync();
        var before = await db.UserRoleAssignments.CountAsync();
        var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "scripts", "branch-access-rollout.sql"));
        await db.Database.ExecuteSqlRawAsync(script);
        Assert.Equal(before, await db.UserRoleAssignments.CountAsync());

        var apply = script.Replace("DECLARE @Apply bit = 0;", "DECLARE @Apply bit = 1;")
            .Replace("DECLARE @CentralUsers TABLE (UserId int PRIMARY KEY);",
                $"DECLARE @CentralUsers TABLE (UserId int PRIMARY KEY); INSERT @CentralUsers VALUES ({central.UserID});");
        await db.Database.ExecuteSqlRawAsync(apply);
        await db.Database.ExecuteSqlRawAsync(apply); // A retry must not duplicate or resurrect grants.
        var resolver = new PensionMS.Infrastructure.Services.Security.ScopedPermissionResolver(db);
        Assert.True((await resolver.GetAllowedScopeAsync(central.UserID, "System.ManageUsers")).AllBranches);
        Assert.Equal(new[] { branch.PensionBranchID }, (await resolver.GetAllowedScopeAsync(legacy.UserID, "System.ManageUsers")).BranchIds);
        Assert.True((await resolver.GetAllowedScopeAsync(revoked.UserID, "System.ManageUsers")).IsDenied);
        Assert.True((await resolver.GetAllowedScopeAsync(missing.UserID, "System.ManageUsers")).IsDenied);
        Assert.Equal(before + 2, await db.UserRoleAssignments.CountAsync());
        Assert.False(await resolver.HasPermissionAsync(central.UserID, PermissionConstants.FileTransfer));
    }

    [Fact]
    public async Task GlobalRegistry_RemainsVisible_WhileForeignFileAndFinancialDetailsAreHidden()
    {
        await using var db = database.OpenContext();
        var (file, otherBranch, _) = await SeedAsync(db, "registry");
        db.ChangeTracker.Clear(); // A tracked navigation must not make the assertion pass accidentally.
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var access = Access(unit, PermissionScope.Restricted([otherBranch.PensionBranchID]));
        var service = new RetireeService(unit, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IMissingDocumentService>(), NullLogger<RetireeService>.Instance, cache, access);

        Assert.True((await service.GetByIdAsync(file.RetireeID)).IsSuccess);
        var row = Assert.Single((await service.GetAllAsync(1, 50, "registry")).Data!.Items);
        Assert.Equal(file.RetireeID, row.RetireeID);
        Assert.Equal(0, row.FileCount);
        Assert.Null(row.FileID);
        var statement = (await service.GetStatementAsync(file.RetireeID)).Data!;
        Assert.Equal(file.RetireeID, statement.Retiree.RetireeID);
        Assert.Empty(statement.RetirementFiles);
        Assert.Empty(statement.ApprovalItems);
        Assert.Empty(statement.Settlements);
        Assert.Empty(statement.Entitlements);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => service.UpdateAsync(file.RetireeID,
            new PensionMS.Core.DTOs.Retirees.UpdateRetireeDto { FullName = "تعديل غير مسموح" }, 1));
    }

    private static IAccessiblePensionRepository Access(IUnitOfWork unit, PermissionScope scope)
    {
        var access = new Mock<IAccessiblePensionRepository>();
        access.Setup(a => a.ForCurrentOperationAsync())
            .ReturnsAsync(new PensionDataQuery(unit, scope));
        access.Setup(a => a.ForPermissionAsync("File.Create"))
            .ReturnsAsync(new PensionDataQuery(unit, scope));
        return access.Object;
    }

    private static async Task<(RetirementFile File, PensionFundBranch Other, User Actor)> SeedAsync(
        PensionMS.Infrastructure.Data.PensionDbContext db, string suffix)
    {
        var actor = new User { Username = suffix, FullName = "مستخدم اختبار", PasswordHash = "test" };
        var other = new PensionFundBranch { BranchName = "فرع آخر", IsActive = true };
        var file = new RetirementFile
        {
            FileNumber = suffix, StatusID = FileStatusConstants.New,
            PensionBranch = new PensionFundBranch { BranchName = "فرع المصدر", IsActive = true },
            Retiree = new Retiree
            {
                FullName = suffix, NationalID = suffix,
                ApprovalItem = new MinistryApprovalItem
                {
                    FullName = suffix, Salary = 90000,
                    Batch = new MinistryApprovalBatch { ImportedByUser = actor, Status = "Test" }
                }
            }
        };
        db.AddRange(file, other);
        await db.SaveChangesAsync();
        return (file, other, actor);
    }
}
