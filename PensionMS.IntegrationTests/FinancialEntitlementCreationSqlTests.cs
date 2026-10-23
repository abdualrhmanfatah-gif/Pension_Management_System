using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Time;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Testing;

namespace PensionMS.IntegrationTests;

public class FinancialEntitlementCreationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task ConcurrentRequests_UseSettlementSalaryAndCreateOnlyOneEntitlement()
    {
        var (id, actor) = await SeedAsync();
        async Task<int> CreateAsync()
        {
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            var service = Service(unit);
            var response = await service.CreateAsync(new CreateFinancialEntitlementDto
                { RetireeID = id, Amount = 1, EffectiveDate = new DateTime(2026, 1, 1) }, actor);
            Assert.True(response.IsSuccess, response.Message);
            Assert.Equal(120000m, response.Data!.Amount);
            return response.Data.EntitlementID;
        }

        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.Equal(results[0], results[1]);
        await using var verify = database.OpenContext();
        var saved = await verify.FinancialEntitlements.Where(e => e.RetireeID == id).OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(90000m, saved[0].Amount);
        Assert.Equal(new DateTime(2020, 1, 1), saved[0].EffectiveDate);
        Assert.Equal(new DateTime(2025, 12, 31), saved[0].EndDate);
        Assert.Equal(120000m, saved[1].Amount);
        Assert.Null(saved[1].EndDate);
        Assert.Contains("تسوية رتبة", saved[1].Notes!);
    }

    [Fact]
    public async Task NestedCreation_RollbackRestoresOldEntitlementAndRemovesNewOne()
    {
        var (id, actor) = await SeedAsync();
        await using (var seed = database.OpenContext())
        {
            seed.Add(new FinancialEntitlement { RetireeID = id, Amount = 90000,
                EffectiveDate = new DateTime(2020, 1, 1) });
            await seed.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
        {
            using var unit = new UnitOfWork(db);
            await unit.BeginTransactionAsync();
            var response = await Service(unit).CreateAsync(new CreateFinancialEntitlementDto
                { RetireeID = id, Amount = 1, EffectiveDate = new DateTime(2026, 1, 1) }, actor);
            Assert.True(response.IsSuccess, response.Message);
            Assert.True(unit.HasActiveTransaction);
            await unit.RollbackAsync();
        }
        await using var verify = database.OpenContext();
        var saved = Assert.Single(await verify.FinancialEntitlements.Where(e => e.RetireeID == id).ToListAsync());
        Assert.Equal(90000m, saved.Amount);
        Assert.Null(saved.EndDate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_UsesFinancialRankAndRetirementDate(bool hasSettlement)
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var settlement = await db.SettlementItems.SingleAsync(s => s.RetireeID == id);
        var rankId = settlement.ProcessedRankID;
        var retiree = await db.Retirees.Include(r => r.ApprovalItem).SingleAsync(r => r.RetireeID == id);
        var approvalRank = new Rank { RankName = Guid.NewGuid().ToString("N"), BaseSalary = 90000 };
        db.Add(approvalRank);
        await db.SaveChangesAsync();
        retiree.ApprovalItem.RankID = approvalRank.RankID;
        if (!hasSettlement) settlement.IsDeleted = true;
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = FileService(unit, cache);
        var response = await service.CreateAsync(new CreateRetirementFileDto
            { RetireeID = id, BranchID = branch.PensionBranchID }, actor);
        Assert.True(response.IsSuccess, response.Message);
        await using var verify = database.OpenContext();
        var file = await verify.RetirementFiles.SingleAsync(f => f.RetireeID == id);
        var entitlements = await verify.FinancialEntitlements.Where(e => e.RetireeID == id)
            .OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(hasSettlement ? 2 : 1, entitlements.Count);
        Assert.Equal(hasSettlement ? rankId : approvalRank.RankID, file.RankID);
        Assert.Equal(90000m, entitlements[0].Amount);
        Assert.Equal(new DateTime(2020, 1, 1), entitlements[0].EffectiveDate);
        if (hasSettlement)
        {
            Assert.Equal(new DateTime(2025, 12, 31), entitlements[0].EndDate);
            Assert.Equal(new DateTime(2026, 1, 1), entitlements[1].EffectiveDate);
            Assert.Equal(120000m, entitlements[1].Amount);
        }
        Assert.Null(entitlements[^1].EndDate);
    }

    [Theory]
    [InlineData("salary")]
    [InlineData("date")]
    [InlineData("unprocessed")]
    [InlineData("conflict")]
    public async Task FileOpening_StrictFinancialFailureRollsBackFileAndProfile(string reason)
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var retiree = await db.Retirees.Include(r => r.ApprovalItem).SingleAsync(r => r.RetireeID == id);
        if (reason == "salary") retiree.ApprovalItem.Salary = null;
        if (reason == "date") retiree.ApprovalItem.RetirementDate = null;
        if (reason == "unprocessed") retiree.ApprovalItem.IsProcessed = false;
        if (reason == "conflict") db.Add(new FinancialEntitlement { RetireeID = id, Amount = 1,
            EffectiveDate = YemenTime.TodayDateTime });
        var oldPhone = retiree.Phone;
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var response = await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto
            { RetireeID = id, BranchID = branch.PensionBranchID, Phone = "777555666" }, actor);
        Assert.False(response.IsSuccess);
        await using var verify = database.OpenContext();
        Assert.False(await verify.RetirementFiles.AnyAsync(f => f.RetireeID == id));
        Assert.Equal(oldPhone, (await verify.Retirees.FindAsync(id))!.Phone);
        Assert.Equal(reason == "conflict" ? 1 : 0, await verify.FinancialEntitlements.CountAsync(e => e.RetireeID == id));
    }

    [Fact]
    public async Task FileOpening_LaterFailureRollsBackAlreadySavedEntitlement()
    {
        var (id, actor) = await SeedAsync();
        await using var db = database.OpenContext();
        var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
        db.Add(branch);
        await db.SaveChangesAsync();
        using var unit = new UnitOfWork(db);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var compliance = new Mock<IMissingDocumentService>();
        compliance.Setup(c => c.RefreshComplianceCacheAsync(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("فشل نهائي اختباري"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => FileService(unit, cache, compliance.Object)
            .CreateAsync(new CreateRetirementFileDto { RetireeID = id, BranchID = branch.PensionBranchID }, actor));
        await using var verify = database.OpenContext();
        Assert.False(await verify.RetirementFiles.AnyAsync(f => f.RetireeID == id));
        Assert.False(await verify.FinancialEntitlements.AnyAsync(e => e.RetireeID == id));
    }

    [Fact]
    public async Task ConcurrentOpeningAndFinancialCreation_UseSameSourcePeriodsWithoutDuplicates()
    {
        var (id, actor) = await SeedAsync();
        int branchId;
        await using (var db = database.OpenContext())
        {
            var branch = new PensionFundBranch { BranchName = Guid.NewGuid().ToString("N"), IsActive = true };
            db.Add(branch);
            await db.SaveChangesAsync();
            branchId = branch.PensionBranchID;
        }
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task OpenAsync()
        {
            await start.Task;
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var response = await FileService(unit, cache).CreateAsync(new CreateRetirementFileDto
                { RetireeID = id, BranchID = branchId }, actor);
            Assert.True(response.IsSuccess, response.Message);
        }
        async Task CreateFinancialAsync()
        {
            await start.Task;
            await using var db = database.OpenContext();
            using var unit = new UnitOfWork(db);
            var response = await Service(unit).CreateAsync(new CreateFinancialEntitlementDto { RetireeID = id }, actor);
            Assert.True(response.IsSuccess, response.Message);
        }
        var opening = OpenAsync();
        var financial = CreateFinancialAsync();
        start.SetResult(true);
        await Task.WhenAll(opening, financial);
        await using var verify = database.OpenContext();
        Assert.Single(await verify.RetirementFiles.Where(f => f.RetireeID == id).ToListAsync());
        var rows = await verify.FinancialEntitlements.Where(e => e.RetireeID == id)
            .OrderBy(e => e.EffectiveDate).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(new DateTime(2020, 1, 1), rows[0].EffectiveDate);
        Assert.Equal(new DateTime(2025, 12, 31), rows[0].EndDate);
        var current = Assert.Single(rows.Where(e => !e.EndDate.HasValue));
        Assert.Equal(120000m, current.Amount);
        Assert.Equal(new DateTime(2026, 1, 1), current.EffectiveDate);
        Assert.All(rows.GroupBy(e => new { e.Amount, e.EffectiveDate }), group => Assert.Single(group));
    }

    private static RetirementFileService FileService(IUnitOfWork unit, IMemoryCache cache,
        IMissingDocumentService? compliance = null)
    {
        var access = TestPensionAccess.All(unit);
        compliance ??= Mock.Of<IMissingDocumentService>();
        var documents = new DocumentService(unit, Mock.Of<IFileStorageService>(),
            Mock.Of<IEntitlementEngine>(), Mock.Of<IFileReviewService>(), access,
            Mock.Of<IImmutableAuditService>(), NullLogger<DocumentService>.Instance, compliance);
        var retiree = new RetireeService(unit, Mock.Of<IFileStorageService>(), documents, compliance,
            NullLogger<RetireeService>.Instance, cache, access);
        return new RetirementFileService(unit, Mock.Of<IImmutableAuditService>(), compliance,
            documents, access, Mock.Of<IEntitledPersonService>(),
            Mock.Of<INotificationService>(), Mock.Of<IFileReviewService>(), NullLogger<RetirementFileService>.Instance,
            retiree, Service(unit), new PensionSuspensionService(unit, Mock.Of<INotificationService>(), access));
    }

    private static FinancialEntitlementService Service(IUnitOfWork unit) => new(unit,
        Mock.Of<IMissingDocumentService>(), Mock.Of<INotificationService>(),
        Mock.Of<IImmutableAuditService>(), NullLogger<FinancialEntitlementService>.Instance, TestPensionAccess.All(unit));

    private async Task<(int RetireeId, int ActorId)> SeedAsync()
    {
        await using var db = database.OpenContext();
        var suffix = Guid.NewGuid().ToString("N")[..16];
        var actor = new User { Username = suffix, FullName = "اختبار الاستحقاق", PasswordHash = "test" };
        var retiree = new Retiree { FullName = suffix, NationalID = suffix,
            ApprovalItem = new MinistryApprovalItem { FullName = suffix, IsProcessed = true,
                Salary = 90000, RetirementDate = new DateOnly(2020, 1, 1),
                Batch = new MinistryApprovalBatch { ImportedByUser = actor, Status = "Test" } } };
        var rank = new Rank { RankName = suffix, BaseSalary = 120000 };
        db.Add(new SettlementItem { Retiree = retiree, CurrentRank = rank, ProcessedRank = rank,
            Batch = new SettlementsBatch { CreatedByUser = actor, Status = "Test" },
            Decision = new SettlementDecision { CreatedByUser = actor, DecisionNumber = suffix,
                DecisionDate = new DateTime(2026, 1, 1) } });
        await db.SaveChangesAsync();
        return (retiree.RetireeID, actor.UserID);
    }
}
