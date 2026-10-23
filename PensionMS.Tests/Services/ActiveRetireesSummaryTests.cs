using PensionMS.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.Tests.Services;

public class ActiveRetireesSummaryTests
{
    private static IRepository<T> Repository<T>(IEnumerable<T> rows) where T : class
    {
        var repository = new Mock<IRepository<T>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<T>(rows));
        return repository.Object;
    }

    private static ITestBranchScope Scope(int? branch = null, bool hq = true) =>
        Mock.Of<ITestBranchScope>(s => s.GetCurrentBranchId() == branch && s.IsHQ() == hq);

    private static ReportService CreateService(int? branch = null, bool hq = true)
    {
        var retirees = new[]
        {
            new Retiree { RetireeID = 1, FullName = "أحمد", NationalID = "001", Governorate = "عدن" },
            new Retiree { RetireeID = 2, FullName = "علي", NationalID = "002", Governorate = " عدن " },
            new Retiree { RetireeID = 3, FullName = "سالم", NationalID = "003", Governorate = "تعز" },
            new Retiree { RetireeID = 4, FullName = "بلا معاش", NationalID = "004", Governorate = " " },
            new Retiree { RetireeID = 5, FullName = "محذوف", IsDeleted = true, Governorate = "صنعاء" },
            new Retiree { RetireeID = 6, FullName = "متوفى", IsAlive = false, Governorate = "صنعاء" }
        };
        var files = retirees.Where(r => r.RetireeID != 4).Select(r => new RetirementFile
        {
            FileID = r.RetireeID, RetireeID = r.RetireeID, PensionBranchID = r.RetireeID == 3 ? 2 : 1
        }).ToArray();
        foreach (var file in files)
            retirees.Single(r => r.RetireeID == file.RetireeID).RetirementFiles.Add(file);

        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pensions = new[]
        {
            new FinancialEntitlement { EntitlementID = 1, RetireeID = 1, Amount = 500m, CreatedAt = date },
            new FinancialEntitlement { EntitlementID = 2, RetireeID = 1, Amount = 1000m, CreatedAt = date.AddDays(1) },
            new FinancialEntitlement { EntitlementID = 3, RetireeID = 1, Amount = 9999m, CreatedAt = date.AddDays(2), IsDeleted = true },
            new FinancialEntitlement { EntitlementID = 4, RetireeID = 2, Amount = 1500m, CreatedAt = date },
            new FinancialEntitlement { EntitlementID = 5, RetireeID = 2, Amount = 2000m, CreatedAt = date },
            new FinancialEntitlement { EntitlementID = 6, RetireeID = 3, Amount = 3000m, CreatedAt = date },
            new FinancialEntitlement { EntitlementID = 7, RetireeID = 5, Amount = 9999m, CreatedAt = date },
            new FinancialEntitlement { EntitlementID = 8, RetireeID = 6, Amount = 4000m, CreatedAt = date }
        };
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(u => u.Retirees).Returns(Repository(retirees));
        unit.Setup(u => u.RetirementFiles).Returns(Repository(files));
        unit.Setup(u => u.FinancialEntitlements).Returns(Repository(pensions));
        return Service(unit.Object, Scope(branch, hq));
    }

    private static ReportService Service(IUnitOfWork unit, ITestBranchScope scope) =>
        new(unit, Mock.Of<IMissingDocumentService>(), TestPensionAccess.From(unit, scope), new ExcelReportExporter(new ExcelReportTemplate()));

    [Fact]
    public async Task Summary_UsesAllMatchingRowsAndLatestNonDeletedPension()
    {
        var service = CreateService();
        var result = await service.GetActiveRetireesSummaryAsync(new ReportFilter { PageNumber = 2, PageSize = 1 });
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Data!.TotalCount);
        Assert.Equal(10000m, result.Data.TotalPension);
        Assert.Equal(2000m, result.Data.AvgPension);
        Assert.Equal(3, result.Data.GovernorateCount);

        var page = (await service.GetActiveRetireesAsync(new ReportFilter { PageSize = 1 })).Data!;
        Assert.Single(page.Items);
        Assert.Equal(result.Data.TotalCount, page.TotalCount);
        Assert.Equal(1000m, page.Items.Single().MonthlyPension);
    }

    [Fact]
    public async Task Summary_AndRowsUseTheSameAllowedBranchScope()
    {
        var service = CreateService(1, false);
        await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() =>
            service.GetActiveRetireesSummaryAsync(new ReportFilter { BranchId = 2 }));
        var filter = new ReportFilter { BranchId = 1 };
        var summary = (await service.GetActiveRetireesSummaryAsync(filter)).Data!;
        var rows = (await service.GetActiveRetireesAsync(filter)).Data!;
        Assert.Equal(3, summary.TotalCount);
        Assert.Equal(7000m, summary.TotalPension);
        Assert.Equal(2, summary.GovernorateCount);
        Assert.Equal(summary.TotalCount, rows.TotalCount);
        Assert.Equal(summary.TotalPension, rows.Items.Sum(row => row.MonthlyPension));
        Assert.DoesNotContain(rows.Items, row => row.RetireeID == 3);
        Assert.DoesNotContain(rows.Items, row => row.RetireeID == 4);
        Assert.Equal("حي", rows.Items.Single(row => row.RetireeID == 1).Status);
        Assert.Equal("متوفى", rows.Items.Single(row => row.RetireeID == 6).Status);
        Assert.DoesNotContain(rows.Items, row => row.RetireeID == 5);
    }

    [Theory]
    [InlineData("علي", 2000)]
    [InlineData("003", 3000)]
    public async Task Summary_SearchMatchesTheSameRowsAsTheReport(string search, int pension)
    {
        var service = CreateService();
        var filter = new ReportFilter { Search = search };
        var summary = (await service.GetActiveRetireesSummaryAsync(filter)).Data!;
        var rows = (await service.GetActiveRetireesAsync(filter)).Data!;
        Assert.Equal(1, summary.TotalCount);
        Assert.Equal(rows.TotalCount, summary.TotalCount);
        Assert.Equal(pension, summary.TotalPension);
        Assert.Equal(summary.TotalPension, rows.Items.Sum(row => row.MonthlyPension));
        Assert.Equal(summary.TotalPension, summary.AvgPension);
    }

    [Fact]
    public async Task Summary_EmptyResultsReturnRealZeros()
    {
        var summary = (await CreateService().GetActiveRetireesSummaryAsync(new ReportFilter { Search = "غير موجود" })).Data!;
        Assert.Equal(0, summary.TotalCount);
        Assert.Equal(0m, summary.TotalPension);
        Assert.Equal(0m, summary.AvgPension);
        Assert.Equal(0, summary.GovernorateCount);
    }

    [Fact]
    public async Task Report_RejectsMissingPermissionScope()
    {
        var service = CreateService(null, false);
        await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() => service.GetActiveRetireesSummaryAsync(new ReportFilter()));
        await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() => service.GetActiveRetireesAsync(new ReportFilter()));
    }

    [Fact]
    public void Summary_AggregationTranslatesOnSqlServerWithoutPagingOrConnection()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMSStructureTests;Integrated Security=True").Options);
        using var unit = new UnitOfWork(db);
        var sql = Service(unit, Scope(1, false)).BuildActiveRetireesSummaryQuery(TestPensionAccess.Query(unit, Scope(1, false)), new ReportFilter
        {
            Search = "أحمد", PageNumber = 5, PageSize = 1
        }).ToQueryString();
        Assert.Contains("SUM(", sql);
        Assert.Contains("AVG(", sql);
        Assert.Contains("COUNT(DISTINCT", sql);
        Assert.Contains("LEFT JOIN", sql);
        Assert.Contains("PensionBranchID", sql);
        Assert.DoesNotContain("OFFSET", sql);
    }
}
