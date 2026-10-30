using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class InvestmentProjectAuthorizationTests
{
    [Fact]
    public async Task ReadsAndSummary_OnlyExposeAuthorizedProjectsPlansAndProperties()
    {
        var f = new Fixture();
        Assert.Equal(1, (await f.Service.GetProjectByIdAsync(1)).Data!.ProjectID);
        Assert.False((await f.Service.GetProjectByIdAsync(2)).IsSuccess);
        Assert.Equal(1, Assert.Single((await f.Service.GetAllProjectsAsync()).Data!).ProjectID);
        Assert.Equal(1, Assert.Single((await f.Service.GetProjectsByPortfolioAsync(1)).Data!).ProjectID);
        Assert.Empty((await f.Service.GetProjectsByPortfolioAsync(2)).Data!);
        var summary = (await f.Service.GetProjectSummaryAsync(1)).Data!;
        Assert.Equal(1, summary.PropertyCount);
        Assert.Equal(100, summary.PlannedBudget);
        Assert.Equal(0, summary.TotalActualSpent);
        Assert.Equal(0, summary.ProgressPercentage);
        Assert.False((await f.Service.GetProjectSummaryAsync(2)).IsSuccess);
        Assert.Equal(1, Assert.Single((await f.Service.GetProjectPlansAsync(1)).Data!).PlanID);
        Assert.Empty((await f.Service.GetProjectPlansAsync(2)).Data!);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CentralScope_CanReadBothBranchesAndCreateInAnEmptyPortfolio()
    {
        var f = new Fixture(central: true);
        Assert.Equal(2, (await f.Service.GetAllProjectsAsync()).Data!.Count);
        var dto = f.ProjectDto(2);
        dto.PortfolioID = 3;
        Assert.True((await f.Service.CreateProjectAsync(dto, 7)).IsSuccess);
        Assert.Contains(f.Projects, p => p.Name == dto.Name && p.PensionBranchID == 2 && p.PortfolioID == 3);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("move")]
    public async Task UnauthorizedDestinationBranch_ThrowsBeforeAnyMutation(string operation)
    {
        var f = new Fixture();
        await Assert.ThrowsAsync<BranchScopeAccessException>(async () =>
        {
            if (operation == "create") await f.Service.CreateProjectAsync(f.ProjectDto(2), 7);
            else await f.Service.UpdateProjectAsync(1, f.UpdateDto(2), 7);
        });
        Assert.Equal(1, f.Projects[0].PensionBranchID);
        Assert.Equal("مشروع أ", f.Projects[0].Name);
        Assert.Equal(2, f.Projects.Count);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData("update")]
    [InlineData("delete")]
    [InlineData("create-plan")]
    [InlineData("update-plan")]
    [InlineData("delete-plan")]
    public async Task ForeignProjectOrPlan_IsHiddenAndCannotBeMutated(string operation)
    {
        var f = new Fixture();
        var success = operation switch
        {
            "update" => (await f.Service.UpdateProjectAsync(2, f.UpdateDto(1), 7)).IsSuccess,
            "delete" => (await f.Service.DeleteProjectAsync(2, 7)).IsSuccess,
            "create-plan" => (await f.Service.CreatePlanAsync(new CreateInvestmentPlanDto { ProjectID = 2 }, 7)).IsSuccess,
            "update-plan" => (await f.Service.UpdatePlanAsync(2, new UpdateInvestmentPlanDto { PlannedAmount = 999 }, 7)).IsSuccess,
            _ => (await f.Service.DeletePlanAsync(2, 7)).IsSuccess
        };
        Assert.False(success);
        Assert.Equal("مشروع ب", f.Projects[1].Name);
        Assert.False(f.Projects[1].IsDeleted);
        Assert.False(f.Plans[1].IsDeleted);
        Assert.Equal(20, f.Plans[1].PlannedAmount);
        Assert.Equal(2, f.Plans.Count);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    [InlineData(false, 4)]
    [InlineData(true, 4)]
    public async Task PortfolioReference_UsesExistingScopeForForeignEmptyAndMixedPortfolios(bool update, int portfolioId)
    {
        var f = new Fixture();
        bool success;
        if (update)
        {
            var dto = f.UpdateDto(1);
            dto.PortfolioID = portfolioId;
            success = (await f.Service.UpdateProjectAsync(1, dto, 7)).IsSuccess;
        }
        else
        {
            var dto = f.ProjectDto(1);
            dto.PortfolioID = portfolioId;
            success = (await f.Service.CreateProjectAsync(dto, 7)).IsSuccess;
        }
        Assert.False(success);
        Assert.Equal(1, f.Projects[0].PortfolioID);
        Assert.Equal("مشروع أ", f.Projects[0].Name);
        Assert.Equal(2, f.Projects.Count);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task AuthorizedProject_CreateUpdateAndSoftDelete_PreserveExistingFields()
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreateProjectAsync(f.ProjectDto(1), 7)).IsSuccess);
        var created = f.Projects.Last();
        Assert.Equal(1, created.PensionBranchID);
        Assert.True((await f.Service.UpdateProjectAsync(1, f.UpdateDto(1), 7)).IsSuccess);
        Assert.Equal("مشروع معدل", f.Projects[0].Name);
        Assert.True((await f.Service.DeleteProjectAsync(1, 7)).IsSuccess);
        Assert.True(f.Projects[0].IsDeleted);
        Assert.NotNull(f.Projects[0].DeletedAt);
        Assert.Equal(7, f.Projects[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task AuthorizedPlan_CreateUpdateAndSoftDelete_DoesNotIntroduceReparenting()
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreatePlanAsync(new CreateInvestmentPlanDto
        { ProjectID = 1, FiscalYearID = 1, PlannedAmount = 30, ActualSpent = 5 }, 7)).IsSuccess);
        Assert.Equal(1, f.Plans.Last().ProjectID);
        Assert.True((await f.Service.UpdatePlanAsync(1, new UpdateInvestmentPlanDto
        { ProjectID = 2, FiscalYearID = 1, PlannedAmount = 40, ActualSpent = 6 }, 7)).IsSuccess);
        Assert.Equal(1, f.Plans[0].ProjectID); // The existing update contract does not move plans.
        Assert.Equal(40, f.Plans[0].PlannedAmount);
        Assert.True((await f.Service.DeletePlanAsync(1, 7)).IsSuccess);
        Assert.True(f.Plans[0].IsDeleted);
        Assert.Equal(7, f.Plans[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task MultipleAuthorizedBranches_AllowBranchReassignment()
    {
        var f = new Fixture(branches: [1, 2]);
        Assert.True((await f.Service.UpdateProjectAsync(1, f.UpdateDto(2), 7)).IsSuccess);
        Assert.Equal(2, f.Projects[0].PensionBranchID);
    }

    [Fact]
    public async Task MissingPermission_FailsClosedEvenWithACallerSuppliedActor()
    {
        var f = new Fixture(branches: []);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.GetAllProjectsAsync());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.CreateProjectAsync(f.ProjectDto(1), 1));
        f.AssertNoWrites();
    }

    [Fact]
    public async Task ExistingUniqueNameRule_IsPreservedWithoutSaving()
    {
        var f = new Fixture();
        var dto = f.ProjectDto(1);
        dto.Name = f.Projects[0].Name;
        Assert.False((await f.Service.CreateProjectAsync(dto, 7)).IsSuccess);
        f.AssertNoWrites();
    }

    [Fact]
    public void ProductionScopeQueries_TranslateForProjectsPlansPortfoliosAndSummaryProperties()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        using var unit = new UnitOfWork(db);
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var queries = TestPensionAccess.Query(unit, identity.Object);
        foreach (var sql in new[]
        {
            queries.Query(unit.InvestmentProjects).Where(p => p.ProjectID == 1).ToQueryString(),
            queries.Query(unit.InvestmentPlans).Where(p => p.ProjectID == 1).ToQueryString(),
            queries.Query(unit.InvestmentPortfolios).Where(p => p.PortfolioID == 1).ToQueryString(),
            queries.Query(unit.InvestmentProperties).Where(p => p.ProjectID == 1).ToQueryString()
        }) Assert.Contains("PensionBranchID", sql);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new(MockBehavior.Strict);
        public List<InvestmentProject> Projects { get; }
        public List<InvestmentPlan> Plans { get; }
        public InvestmentProjectService Service { get; }
        private readonly List<Action> _verifyNoWrites = [];

        public Fixture(bool central = false, int[]? branches = null)
        {
            var a = new InvestmentProject { ProjectID = 1, PortfolioID = 1, PensionBranchID = 1, Name = "مشروع أ", PlannedBudget = 100 };
            var b = new InvestmentProject { ProjectID = 2, PortfolioID = 2, PensionBranchID = 2, Name = "مشروع ب" };
            Projects = [a, b];
            var portfolios = new List<InvestmentPortfolio>
            {
                new() { PortfolioID = 1, Name = "محفظة أ", Projects = [a] },
                new() { PortfolioID = 2, Name = "محفظة ب", Projects = [b] },
                new() { PortfolioID = 3, Name = "محفظة فارغة" },
                new() { PortfolioID = 4, Name = "محفظة مشتركة", Projects = [a, b] }
            };
            a.Portfolio = portfolios[0];
            b.Portfolio = portfolios[1];
            Plans = [new() { PlanID = 1, ProjectID = 1, Project = a, PlannedAmount = 10 },
                new() { PlanID = 2, ProjectID = 2, Project = b, PlannedAmount = 20 }];
            Bind(u => u.RetirementFiles, new List<RetirementFile>());
            Bind(u => u.Retirees, new List<Retiree>());
            Bind(u => u.InvestmentProjects, Projects);
            Bind(u => u.InvestmentPlans, Plans);
            Bind(u => u.InvestmentPortfolios, portfolios);
            Bind(u => u.InvestmentTypes, new List<InvestmentType> { new() { TypeID = 1 } });
            Bind(u => u.InvestmentProperties, new List<InvestmentProperty>
            {
                new() { PropertyID = 1, ProjectID = 1, PensionBranchID = 1 },
                new() { PropertyID = 2, ProjectID = 1, PensionBranchID = 2 },
                new() { PropertyID = 3, ProjectID = 1, PensionBranchID = 1, IsDeleted = true }
            });
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            var access = new TestPensionAccess(Unit.Object, () => central
                ? PensionMS.Core.Security.PermissionScope.All
                : PensionMS.Core.Security.PermissionScope.Restricted(branches ?? [1]));
            Service = new InvestmentProjectService(Unit.Object, access);
        }

        public CreateProjectDto ProjectDto(int branch) => new()
        { Name = "مشروع جديد", PensionBranchID = branch, PortfolioID = 1, TypeID = 1, PlannedBudget = 100 };
        public UpdateProjectDto UpdateDto(int branch) => new()
        { Name = "مشروع معدل", PensionBranchID = branch, PortfolioID = 1, TypeID = 1, PlannedBudget = 100 };

        public void AssertNoWrites()
        {
            Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
            foreach (var verify in _verifyNoWrites) verify();
        }

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> selector, List<T> rows) where T : class
        {
            var repository = new Mock<IRepository<T>>(MockBehavior.Strict);
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(rows.FirstOrDefault());
            repository.Setup(r => r.AddAsync(It.IsAny<T>())).Callback<T>(rows.Add).Returns(Task.CompletedTask);
            repository.Setup(r => r.Update(It.IsAny<T>()));
            Unit.Setup(selector).Returns(repository.Object);
            _verifyNoWrites.Add(() =>
            {
                repository.Verify(r => r.AddAsync(It.IsAny<T>()), Times.Never);
                repository.Verify(r => r.Update(It.IsAny<T>()), Times.Never);
            });
        }
    }
}
