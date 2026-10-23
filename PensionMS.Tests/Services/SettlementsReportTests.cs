using PensionMS.Testing;
using Moq;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.Tests.Services;

public class SettlementsReportTests
{
    private static ITestBranchScope Scope(int? branch, bool hq = false)
    {
        var mock = new Mock<ITestBranchScope>();
        mock.Setup(s => s.GetCurrentBranchId()).Returns(branch);
        mock.Setup(s => s.IsHQ()).Returns(hq);
        return mock.Object;
    }

    private static SettlementDecision Decision(int id, string number, DateTime date, string? type) =>
        new() { DecisionID = id, DecisionNumber = number, DecisionDate = date, DecisionType = type, CreatedByUserID = 1 };

    private static SettlementItem Item(
        int id, SettlementDecision? decision, string name, string? military,
        string prevRank, string procRank, int retireeId = 0) => new()
    {
        SettlementItemID = id,
        BatchID = 1,
        Decision = decision,
        DecisionID = decision?.DecisionID,
        FullName = name,
        MilitaryNumber = military,
        RetireeID = retireeId > 0 ? retireeId : null,
        CurrentRank = new Rank { RankID = 1, RankName = prevRank },
        ProcessedRank = new Rank { RankID = 2, RankName = procRank },
        Amount = 1000m
    };

    private static readonly SettlementDecision D1 = Decision(1, "Q-2026-1", new DateTime(2026, 1, 10), "تسوية رتب");
    private static readonly SettlementDecision D2 = Decision(2, "Q-2026-2", new DateTime(2026, 3, 5), "إعادة احتساب");

    private static List<SettlementItem> SampleItems()
    {
        var deleted = Item(5, D2, "محذوف", null, "رائد", "مقدم");
        deleted.IsDeleted = true;
        return
        [
            Item(1, D1, "أحمد علي", "M-111", "عقيد", "عقيد ركن", retireeId: 10),
            Item(2, D1, "محمد سعيد", "M-222", "مقدم", "عقيد"),
            Item(3, D2, "سالم ناصر", "M-333", "رائد", "مقدم", retireeId: 99),
            Item(4, null, "بلا قرار", null, "نقيب", "رائد"),
            deleted
        ];
    }

    private static (ReportService Service, IPensionDataQuery Queries) CreateService(List<SettlementItem> items, ITestBranchScope scope)
    {
        var itemsRepo = new Mock<IRepository<SettlementItem>>();
        itemsRepo.Setup(r => r.Query()).Returns(items.AsQueryable());

        var filesRepo = new Mock<IRepository<RetirementFile>>();
        filesRepo.Setup(r => r.Query()).Returns(new[]
        {
            new RetirementFile { FileID = 1, RetireeID = 10, PensionBranchID = 1 }
        }.AsQueryable());

        var retireesRepo = new Mock<IRepository<Retiree>>();
        retireesRepo.Setup(r => r.Query()).Returns(new[]
        {
            new Retiree { RetireeID = 10 }
        }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.SettlementItems).Returns(itemsRepo.Object);
        uow.Setup(u => u.RetirementFiles).Returns(filesRepo.Object);
        uow.Setup(u => u.Retirees).Returns(retireesRepo.Object);

        return (new ReportService(uow.Object, Mock.Of<IMissingDocumentService>(), TestPensionAccess.From(uow.Object, scope), new ExcelReportExporter(new ExcelReportTemplate())), TestPensionAccess.Query(uow.Object, scope));
    }

    private static readonly ReportFilter HqFilter = new();

    [Fact]
    public void NoFilters_ReturnsOnlyVisibleDecisionItems()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        var rows = service.Service.BuildSettlementItemsQuery(service.Queries, HqFilter).ToList();
        Assert.Equal(3, rows.Count);
        Assert.DoesNotContain(rows, i => i.IsDeleted);
        Assert.DoesNotContain(rows, i => i.DecisionID == null);
    }

    [Fact]
    public void FromDate_ExcludesEarlierDecisions()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        var filter = new ReportFilter { FromDate = new DateTime(2026, 2, 1) };
        var rows = service.Service.BuildSettlementItemsQuery(service.Queries, filter).ToList();
        var only = Assert.Single(rows);
        Assert.Equal(3, only.SettlementItemID);
    }

    [Fact]
    public void ToDate_IncludesWholeBoundaryDay()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        var filter = new ReportFilter { ToDate = new DateTime(2026, 1, 10) };
        var rows = service.Service.BuildSettlementItemsQuery(service.Queries, filter).ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, i => Assert.Equal(1, i.DecisionID));
    }

    [Fact]
    public void DecisionType_MatchesPartially()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        var filter = new ReportFilter { DecisionType = "تسوية" };
        var rows = service.Service.BuildSettlementItemsQuery(service.Queries, filter).ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, i => Assert.Equal("Q-2026-1", i.Decision!.DecisionNumber));
    }

    [Fact]
    public void Search_MatchesDecisionNumberNameAndMilitaryNumber()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        Assert.Single(service.Service.BuildSettlementItemsQuery(service.Queries, new ReportFilter { Search = "Q-2026-2" }).ToList());
        Assert.Single(service.Service.BuildSettlementItemsQuery(service.Queries, new ReportFilter { Search = "سالم" }).ToList());
        Assert.Single(service.Service.BuildSettlementItemsQuery(service.Queries, new ReportFilter { Search = "M-222" }).ToList());
    }

    [Fact]
    public void Projection_CarriesRanksMilitaryNumberAndType()
    {
        var service = CreateService(SampleItems(), Scope(null, hq: true));
        var query = service.Service.BuildSettlementItemsQuery(service.Queries, new ReportFilter { Search = "M-111" });
        var row = Assert.Single(ReportService.ProjectSettlements(query).ToList());
        Assert.Equal(1, row.SettlementID);
        Assert.Equal(1, row.DecisionID);
        Assert.Equal("Q-2026-1", row.DecisionNumber);
        Assert.Equal("أحمد علي", row.RetireeName);
        Assert.Equal("M-111", row.MilitaryNumber);
        Assert.Equal("عقيد", row.PreviousRank);
        Assert.Equal("عقيد ركن", row.ProcessedRank);
        Assert.Equal("تسوية رتب", row.Status);
        Assert.Equal(new DateTime(2026, 1, 10), row.DecisionDate);
    }

    [Fact]
    public void BranchScope_LimitsItemsToBranchRetirees()
    {
        var service = CreateService(SampleItems(), Scope(1));
        var rows = service.Service.BuildSettlementItemsQuery(service.Queries, HqFilter).ToList();
        var only = Assert.Single(rows);
        Assert.Equal(1, only.SettlementItemID);
    }

    [Fact]
    public void BranchScope_ExcludesAllWhenNoMatch()
    {
        var service = CreateService(SampleItems(), Scope(2));
        Assert.Empty(service.Service.BuildSettlementItemsQuery(service.Queries, HqFilter).ToList());
    }
}
