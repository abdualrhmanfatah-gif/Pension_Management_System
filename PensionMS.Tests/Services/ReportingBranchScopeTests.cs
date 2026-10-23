using PensionMS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PensionMS.Core.DTOs;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.Tests.Services;

public class ReportingBranchScopeTests
{
    private static ITestBranchScope Scope(int? branch, bool hq = false)
    {
        var mock = new Mock<ITestBranchScope>();
        mock.Setup(s => s.GetCurrentBranchId()).Returns(branch);
        mock.Setup(s => s.IsHQ()).Returns(hq);
        return mock.Object;
    }

    [Fact]
    public void FilesAndRetirees_AreRestrictedToCurrentBranch()
    {
        var uow = new Mock<IUnitOfWork>();
        var files = new Mock<IRepository<RetirementFile>>();
        var retirees = new Mock<IRepository<Retiree>>();
        files.Setup(r => r.Query()).Returns(new[]
        {
            new RetirementFile { FileID = 1, RetireeID = 10, PensionBranchID = 1 },
            new RetirementFile { FileID = 2, RetireeID = 20, PensionBranchID = 2 }
        }.AsQueryable());
        retirees.Setup(r => r.Query()).Returns(new[]
        {
            new Retiree { RetireeID = 10 }, new Retiree { RetireeID = 20 }
        }.AsQueryable());
        uow.Setup(u => u.RetirementFiles).Returns(files.Object);
        uow.Setup(u => u.Retirees).Returns(retirees.Object);
        var queries = TestPensionAccess.Query(uow.Object, Scope(1));
        Assert.Equal(1, Assert.Single(queries.Query(files.Object)).FileID);
        Assert.Equal(10, Assert.Single(queries.Query(retirees.Object)).RetireeID);
        Assert.Equal(2, TestPensionAccess.Query(uow.Object, Scope(null, true)).Query(files.Object).Count());
    }

    [Fact]
    public void InvalidOrForeignScope_IsRejectedBeforeReadingData()
    {
        var uow = Mock.Of<IUnitOfWork>();
        Assert.Throws<BranchScopeAccessException>(() => TestPensionAccess.Query(uow, Scope(null)).ResolveBranch());
        Assert.Throws<BranchScopeAccessException>(() => TestPensionAccess.Query(uow, Scope(1)).ResolveBranch(2));
        Assert.Throws<ArgumentException>(() => TestPensionAccess.Query(uow, Scope(1)).ResolveBranch(0));
        Assert.Equal(2, TestPensionAccess.Query(uow, Scope(null, true)).ResolveBranch(2));
    }

    [Fact]
    public async Task DashboardCache_DoesNotShareStatsAcrossBranchesOrHQ()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("DashboardStats:1", new DashboardStatsDto { TotalRetirees = 10 });
        cache.Set("DashboardStats:2", new DashboardStatsDto { TotalRetirees = 20 });
        cache.Set("DashboardStats:all", new DashboardStatsDto { TotalRetirees = 30 });
        var uow = Mock.Of<IUnitOfWork>();
        Assert.Equal(10, (await new DashboardService(uow, cache, TestPensionAccess.From(uow, Scope(1))).GetStatsAsync()).Data!.TotalRetirees);
        Assert.Equal(20, (await new DashboardService(uow, cache, TestPensionAccess.From(uow, Scope(2))).GetStatsAsync()).Data!.TotalRetirees);
        Assert.Equal(30, (await new DashboardService(uow, cache, TestPensionAccess.From(uow, Scope(null, true))).GetStatsAsync()).Data!.TotalRetirees);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => new DashboardService(uow, cache, TestPensionAccess.From(uow, Scope(null))).GetStatsAsync());
    }

    [Fact]
    public void DashboardStatisticsAggregates_TranslateOnSqlServerWithoutConnection()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMSStructureTests;Integrated Security=True").Options);
        using var uow = new UnitOfWork(db);
        var queries = TestPensionAccess.Query(uow, Scope(2));
        var files = queries.Query(uow.RetirementFiles).Where(f => !f.IsDeleted);
        Assert.All(new[]
        {
            files.GroupBy(f => f.StatusID).Select(g => new { StatusID = g.Key, Count = g.Count() }).ToQueryString(),
            files.GroupBy(f => f.Rank.RankName ?? "بدون رتبة").Select(g => new { RankName = g.Key, Count = g.Count() }).ToQueryString(),
            files.Where(f => f.Retiree.Governorate != null && f.Retiree.Governorate != "")
                .GroupBy(f => f.Retiree.Governorate!)
                .Select(g => new { g.Key, Count = g.Count() }).ToQueryString(),
            files.Where(f => f.Retiree.ApprovalItem.Category != null)
                .GroupBy(f => f.Retiree.ApprovalItem.Category!.CategoryName)
                .Select(g => new { g.Key, Count = g.Count() }).ToQueryString(),
            queries.Query(uow.SettlementItems).Where(s => !s.IsDeleted && s.Retiree != null && !s.Retiree.IsDeleted)
                .GroupBy(s => s.Retiree.Governorate)
                .Select(g => new { Governorate = g.Key, Count = g.Count(), TotalAmount = g.Sum(s => s.Amount - (s.Deductions ?? 0m)) })
                .ToQueryString(),
            queries.Query(uow.Retirees).Where(r => !r.IsDeleted)
                .GroupBy(r => r.CreatedAt.Month).Select(g => new { Month = g.Key, Count = g.Count() }).ToQueryString()
        }, sql => Assert.False(string.IsNullOrWhiteSpace(sql)));
    }

    [Fact]
    public void ScopedAuditDocumentsAndBatchQueries_TranslateOnSqlServerWithoutConnection()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMSStructureTests;Integrated Security=True").Options);
        using var uow = new UnitOfWork(db);
        var queries = TestPensionAccess.Query(uow, Scope(2));
        foreach (var sql in new[]
        {
            queries.Query(uow.AuditLogs).ToQueryString(),
            queries.Query(uow.Documents).ToQueryString(),
            queries.Query(uow.SalaryPaymentBatches).ToQueryString(),
            queries.Query(uow.MinistryApprovalItems).ToQueryString()
        })
            Assert.Contains("PensionBranchID", sql);
    }
}
