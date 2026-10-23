using PensionMS.Infrastructure.Services.Settlements;
using System.Linq.Expressions;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Tests.Services;

public class SettlementFinancialEligibilityTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISettlementRepository> _settlements = new();
    private readonly Mock<IFinancialEntitlementService> _entitlementService = new();
    private readonly List<FinancialEntitlement> _entitlements = new();
    private readonly Retiree _retiree = new()
    {
        RetireeID = 217,
        MilitaryNumber = "4000585",
        PendingReviewStatus = PendingReviewStatus.Pending,
        ApprovalItemID = 11,
        ApprovalItem = new MinistryApprovalItem { ApprovalItemID = 11, IsProcessed = true, Salary = 27946 }
    };
    private readonly SettlementItem _item = new()
    {
        SettlementItemID = 5725,
        ProcessedRankID = 1,
        ProcessedRank = new Rank { RankID = 1, BaseSalary = 120000 },
        DecisionID = 12,
        Decision = new SettlementDecision { DecisionID = 12, DecisionNumber = "سياسي42-12", DecisionDate = new DateTime(2026, 1, 1) }
    };

    public SettlementFinancialEligibilityTests()
    {
        Query(u => u.Retirees, new[] { _retiree });
        Query(u => u.SettlementItems, new[] { _item });
        Query(u => u.RetirementFiles, Array.Empty<RetirementFile>());
        var financialRepo = Query(u => u.FinancialEntitlements, _entitlements);
        financialRepo.Setup(r => r.AddAsync(It.IsAny<FinancialEntitlement>()))
            .Callback<FinancialEntitlement>(e => { e.EntitlementID = _entitlements.Count + 1; e.Retiree = _retiree; _entitlements.Add(e); })
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _settlements.Setup(s => s.ApplyRetireeRankUpdatesAsync(It.IsAny<List<RetireeRankUpdate>>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Link_FinancialFailure_DoesNotReportSuccessOrUpdateRank()
    {
        _entitlementService.Setup(s => s.CreateAsync(It.IsAny<CreateFinancialEntitlementDto>(), 7))
            .ReturnsAsync(ApiResponse<FinancialEntitlementDto>.Fail("تعذر حفظ الاستحقاق"));
        var service = SettlementService();

        var result = await service.LinkItemToRetireeAsync(new LinkSettlementItemDto { SettlementItemID = 5725, RetireeID = 217 }, 7);

        Assert.False(result.IsSuccess);
        Assert.Contains("تعذر حفظ الاستحقاق", result.Message);
        _unitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _settlements.Verify(s => s.ApplyRetireeRankUpdatesAsync(It.IsAny<List<RetireeRankUpdate>>()), Times.Never);
    }

    [Fact]
    public async Task Link_ExistingLinkedItem_RepairsMissingEntitlement()
    {
        _item.RetireeID = _retiree.RetireeID;
        _entitlementService.Setup(s => s.CreateAsync(It.IsAny<CreateFinancialEntitlementDto>(), 7))
            .ReturnsAsync(ApiResponse<FinancialEntitlementDto>.Success(new FinancialEntitlementDto()));

        var result = await SettlementService().LinkItemToRetireeAsync(
            new LinkSettlementItemDto { SettlementItemID = 5725, RetireeID = 217 }, 7);

        Assert.True(result.IsSuccess, result.Message);
        _entitlementService.Verify(s => s.CreateAsync(It.Is<CreateFinancialEntitlementDto>(d =>
            d.RetireeID == 217 && d.Amount == 120000m && d.EffectiveDate == new DateTime(2026, 1, 1)), 7), Times.Once);
        _settlements.Verify(s => s.ApplyRetireeRankUpdatesAsync(It.IsAny<List<RetireeRankUpdate>>()), Times.Once);
    }

    [Fact]
    public async Task Link_ExistingEntitlement_DoesNotCreateDuplicate()
    {
        _item.RetireeID = _retiree.RetireeID;
        _entitlements.Add(new FinancialEntitlement
        {
            RetireeID = 217, Amount = 120000m, EffectiveDate = new DateTime(2026, 1, 1),
            Notes = "تسوية رتبة — قرار رقم سياسي42-12"
        });

        var result = await SettlementService().LinkItemToRetireeAsync(
            new LinkSettlementItemDto { SettlementItemID = 5725, RetireeID = 217 }, 7);

        Assert.True(result.IsSuccess, result.Message);
        _entitlementService.Verify(s => s.CreateAsync(It.IsAny<CreateFinancialEntitlementDto>(), It.IsAny<int>()), Times.Never);
    }

    private SettlementService SettlementService() => SettlementServiceFactory.Create(
        _unitOfWork, settlements: _settlements, entitlements: _entitlementService);

    private Mock<IRepository<T>> Query<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> data) where T : class
    {
        var repo = new Mock<IRepository<T>>();
        repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(data));
        _unitOfWork.Setup(property).Returns(repo.Object);
        return repo;
    }
}
