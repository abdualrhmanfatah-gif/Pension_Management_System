using PensionMS.Testing;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Tests.Services;

public class AgentFeatureTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task UnusedAgency_PersistsSelectedGeneralScope(int scope)
    {
        var f = new Fixture();
        f.Recipients.Clear();
        var result = await f.AgentService.UpdateAsync(1, new UpdateAgentDto
        { UpdateEntitledScope = true, AgencyScope = scope, FullName = "اسم معدل", NationalID = "123", AgencyNumber = "A1", AgencyDate = YemenTime.TodayDateTime }, 7);
        Assert.True(result.IsSuccess);
        Assert.Equal(scope, result.Data!.AgencyScope);
        Assert.Null(result.Data.EntitledID);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, null)]
    [InlineData(9, null)]
    public async Task InvalidExplicitScope_IsRejectedBeforeMutation(int scope, int? entitledId)
    {
        var f = new Fixture();
        var result = await f.AgentService.UpdateAsync(1, new UpdateAgentDto
        { UpdateEntitledScope = true, AgencyScope = scope, EntitledID = entitledId, FullName = "اسم معدل", NationalID = "123" }, 7);
        Assert.False(result.IsSuccess);
        Assert.Equal(1, f.Agents[0].EntitledID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangingUsedAgencyBetweenGeneralScopes_IsRejected()
    {
        var f = new Fixture();
        f.Agents[0].EntitledID = null;
        f.Agents[0].AgencyScope = AgentScope.Retiree;
        var result = await f.AgentService.UpdateAsync(1, new UpdateAgentDto
        { UpdateEntitledScope = true, AgencyScope = 2, FullName = "اسم معدل", NationalID = "123" }, 7);
        Assert.False(result.IsSuccess);
        Assert.Equal(AgentScope.Retiree, f.Agents[0].AgencyScope);
    }

    [Fact]
    public async Task PersonalUpdate_PreservesExplicitScope()
    {
        var f = new Fixture();
        f.Agents[0].AgencyScope = AgentScope.SpecificEntitled;
        var result = await f.AgentService.UpdateAsync(1, new UpdateAgentDto
        { FullName = "اسم معدل", NationalID = "123", AgencyNumber = "A1", AgencyDate = YemenTime.TodayDateTime }, 7);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data!.AgencyScope);
        Assert.Equal(1, result.Data.EntitledID);
    }

    [Fact]
    public async Task UpdatingPersonalData_PreservesEntitledScope()
    {
        var f = new Fixture();
        var agent = f.Agents[0];
        var result = await f.AgentService.UpdateAsync(agent.AgentID, new UpdateAgentDto
        {
            FullName = "اسم معدل", NationalID = "123456789", AgencyNumber = "A1", AgencyDate = DateTime.UtcNow.AddDays(-3)
        }, 7);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, agent.EntitledID);
        Assert.Equal("اسم معدل", agent.FullName);
    }

    [Fact]
    public async Task MovingUsedAgency_IsRejectedBeforeChangingData()
    {
        var f = new Fixture();
        var result = await f.AgentService.UpdateAsync(1, new UpdateAgentDto
        { UpdateEntitledScope = true, EntitledID = 2, FullName = "اسم آخر", NationalID = "123" }, 7);
        Assert.False(result.IsSuccess);
        Assert.Equal(1, f.Agents[0].EntitledID);
        Assert.NotEqual("اسم آخر", f.Agents[0].FullName);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndingAgency_EndsOnlyItsRecipients_AndPreservesContractExpiry(bool delete)
    {
        var f = new Fixture();
        var expiry = f.Agents[0].AgencyEndDate;
        var result = delete ? await f.AgentService.DeleteAsync(1, 7) : await f.AgentService.DeactivateAsync(1, 7);
        Assert.True(result.IsSuccess);
        Assert.False(f.Recipients[0].IsActive);
        Assert.NotNull(f.Recipients[0].EndDate);
        Assert.True(f.Recipients[1].IsActive);
        Assert.Equal(expiry, f.Agents[0].AgencyEndDate);
    }

    [Fact]
    public async Task ForeignBranchAgency_CannotBeEdited()
    {
        var f = new Fixture();
        f.Scope.Setup(s => s.IsHQ()).Returns(false);
        f.Scope.Setup(s => s.GetCurrentBranchId()).Returns(99);
        var result = await f.AgentService.DeactivateAsync(1, 7);
        Assert.False(result.IsSuccess);
        Assert.True(f.Agents[0].IsActive);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task FailedReview_RollsBackAgencyOperation()
    {
        var f = new Fixture();
        f.Review.Setup(r => r.RequestReviewAsync(1, 7, It.IsAny<DateTime?>()))
            .ReturnsAsync(ApiResponse<bool>.Fail("مراجعة مرفوضة"));
        var result = await f.AgentService.DeactivateAsync(1, 7);
        Assert.False(result.IsSuccess);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task CompleteCoverage_GeneratesExactShares()
    {
        var f = new Fixture();
        var result = await f.Generate();
        Assert.Empty(result.SkippedItems);
        Assert.Equal(new decimal[] { 50000, 30000, 20000 }, result.GeneratedItems.Select(i => i.Amount));
    }

    [Fact]
    public async Task MissingOneRecipient_BlocksWholeFileWithoutRedistribution()
    {
        var f = new Fixture();
        f.Recipients.RemoveAt(2);
        var result = await f.Generate();
        Assert.Empty(result.GeneratedItems);
        Assert.Contains("لم يكتمل", Assert.Single(result.SkippedItems).Reason);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task InvalidAgency_BlocksWholeFile(int invalidKind)
    {
        var f = new Fixture();
        if (invalidKind == 0) f.Agents[0].IsActive = false;
        if (invalidKind == 1) f.Agents[0].AgencyEndDate = YemenTime.TodayDateTime.AddDays(-1);
        if (invalidKind == 2) f.Agents[0].PendingReviewStatus = PendingReviewStatus.Rejected;
        var result = await f.Generate();
        Assert.Empty(result.GeneratedItems);
        Assert.Single(result.SkippedItems);
    }

    [Fact]
    public async Task DuplicateAccount_BlocksFileEvenWithHundredPercent()
    {
        var f = new Fixture();
        f.Recipients[1].Agent = f.Agents[0];
        f.Recipients[1].AgentID = 1;
        var result = await f.Generate();
        Assert.Empty(result.GeneratedItems);
        Assert.Contains("تعارض", Assert.Single(result.SkippedItems).Reason);
    }

    [Fact]
    public async Task IncompleteShares_AreNotNormalized()
    {
        var f = new Fixture();
        f.Recipients[2].SharePercent = 10;
        var result = await f.Generate();
        Assert.Empty(result.GeneratedItems);
        Assert.Contains("100", Assert.Single(result.SkippedItems).Reason);
    }

    [Fact]
    public async Task ExpiryDay_RemainsPayable()
    {
        var f = new Fixture();
        f.Agents[0].AgencyEndDate = YemenTime.TodayDateTime;
        Assert.Equal(3, (await f.Generate()).GeneratedItems.Count);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<ITestBranchScope> Scope { get; } = new();
        public Mock<IFileReviewService> Review { get; } = new();
        public List<Agent> Agents { get; } = new();
        public List<SalaryRecipient> Recipients { get; } = new();
        public AgentService AgentService { get; }

        public Fixture()
        {
            var retiree = new Retiree { RetireeID = 1, IsAlive = false, FullName = "المتقاعد" };
            var file = new RetirementFile { FileID = 1, FileNumber = "F1", PensionBranchID = 1,
                RetireeID = 1, Retiree = retiree, StatusID = FileStatusConstants.Active };
            var persons = new List<EntitledPerson>();
            var percentages = new decimal[] { 50, 30, 20 };
            for (var i = 0; i < 3; i++)
            {
                var person = new EntitledPerson { EntitledID = i + 1, FileID = 1, FullName = "مستحق " + i,
                    IsActive = true, SharePercent = percentages[i], RetirementFile = file };
                persons.Add(person);
                var agent = new Agent { AgentID = i + 1, FileID = 1, RetirementFile = file, FullName = "وكيل " + i,
                    NationalID = "123456789", EntitledID = person.EntitledID, EntitledPerson = person,
                    AgencyNumber = "A" + i, AgencyDate = YemenTime.TodayDateTime.AddDays(-5), AgencyEndDate = YemenTime.TodayDateTime.AddDays(5) };
                Agents.Add(agent);
                Recipients.Add(new SalaryRecipient { RecipientID = i + 1, FileID = 1, AgentID = agent.AgentID,
                    Agent = agent, StartDate = DateTime.UtcNow.AddDays(-2), SharePercent = percentages[i] });
            }
            Bind(new[] { file }, u => u.RetirementFiles);
            Bind(new[] { retiree }, u => u.Retirees);
            Bind(Agents, u => u.Agents);
            Bind(persons, u => u.EntitledPersons);
            Bind(Recipients, u => u.SalaryRecipients);
            Bind(Array.Empty<GuardianEntitledPerson>(), u => u.GuardianEntitledPersons);
            Bind(Array.Empty<PensionSuspension>(), u => u.PensionSuspensions);
            Bind(Array.Empty<SalaryPaymentItem>(), u => u.SalaryPaymentItems);
            Bind(new[] { new PaymentBook { PaymentBookID = 1, FileID = 1, Status = PaymentBookStatus.Active,
                IssueDate = YemenTime.Today.AddDays(-1), ExpiryDate = YemenTime.Today.AddYears(1) } }, u => u.PaymentBooks);
            Bind(new[] { new FinancialEntitlement { EntitlementID = 1, RetireeID = 1, Amount = 100000,
                EffectiveDate = DateTime.UtcNow.AddDays(-5) } }, u => u.FinancialEntitlements);
            Scope.Setup(s => s.IsHQ()).Returns(true);
            Review.Setup(r => r.RequestReviewAsync(1, 7, It.IsAny<DateTime?>())).ReturnsAsync(ApiResponse<bool>.Success(true));
            AgentService = new AgentService(Unit.Object, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
                Mock.Of<IMissingDocumentService>(), Review.Object, NullLogger<AgentService>.Instance, TestPensionAccess.From(Unit.Object, Scope.Object));
        }

        public async Task<PayrollGenerationResult> Generate()
        {
            var file = Unit.Object.RetirementFiles.Query().Single();
            file.LastReviewedFileState = (await ApprovedPensionSnapshot.CaptureAsync(Unit.Object, 1, 1)).Serialize();
            return await new PayrollGenerationService(Unit.Object).GenerateForBatchAsync(
                new SalaryPaymentBatch { BatchID = 1 }, DateTime.UtcNow, 7);
        }

        private void Bind<T>(IEnumerable<T> values, Expression<Func<IUnitOfWork, IRepository<T>>> property) where T : class
        {
            var repository = new Mock<IRepository<T>>();
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(values));
            repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => values.FirstOrDefault());
            Unit.Setup(property).Returns(repository.Object);
        }
    }
}
