using PensionMS.Testing;
using PensionMS.Infrastructure.Services.Administration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Moq;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Tests.Services;

public class LookupServiceTests
{
    [Fact]
    public async Task DocumentTypes_SortsByLiveUsage_ThenName_AndKeepsUnusedTypes()
    {
        using var fixture = new Fixture();
        fixture.Types.AddRange(new[]
        {
            Type(1, "جيم", Doc(), Doc()),
            Type(2, "باء", Doc()),
            Type(3, "ألف", Doc(), Doc(deleted: true), Doc(deleted: true)),
            Type(4, "دال"),
            new DocumentType { DocumentTypeID = 5, IsActive = false },
            new DocumentType { DocumentTypeID = 6, OwnerType = (DocumentOwnerType)15 }
        });

        var result = await fixture.Service.GetDocumentTypesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 3, 2, 4 }, result.Data!.Select(t => t.Id));
    }

    [Fact]
    public async Task DocumentTypes_CachesForOneHour_ReadsDoNotExtendExpiration()
    {
        using var fixture = new Fixture();
        var first = Type(1, "ألف", Doc());
        var second = Type(2, "باء");
        fixture.Types.AddRange(new[] { first, second });
        await fixture.Service.GetDocumentTypesAsync();
        second.Documents.Add(Doc());
        second.Documents.Add(Doc());

        fixture.Clock.UtcNow += TimeSpan.FromMinutes(59);
        var cached = await fixture.Service.GetDocumentTypesAsync();
        Assert.Equal(new[] { 1, 2 }, cached.Data!.Select(t => t.Id));
        fixture.Repository.Verify(r => r.Query(), Times.Once);

        fixture.Clock.UtcNow += TimeSpan.FromMinutes(1);
        var refreshed = await fixture.Service.GetDocumentTypesAsync();
        Assert.Equal(new[] { 2, 1 }, refreshed.Data!.Select(t => t.Id));
        fixture.Repository.Verify(r => r.Query(), Times.Exactly(2));
    }

    [Fact]
    public async Task DocumentTypes_IsolatesBranchAndHeadquartersCaches()
    {
        using var fixture = new Fixture();
        fixture.Types.AddRange(new[]
        {
            Type(1, "ألف", Doc(branch: 1)),
            Type(2, "باء", Doc(branch: 2), Doc(branch: 2)),
            Type(3, "جيم", Doc(), Doc(), Doc())
        });

        fixture.Scope.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var headquarters = await fixture.Service.GetDocumentTypesAsync();
        Assert.Equal(new[] { 3, 2, 1 }, headquarters.Data!.Select(t => t.Id));

        fixture.Scope.Setup(s => s.IsHQ()).Returns(false);
        var branchOne = await fixture.Service.GetDocumentTypesAsync();
        Assert.Equal(new[] { 1, 2, 3 }, branchOne.Data!.Select(t => t.Id));

        fixture.Scope.Setup(s => s.GetCurrentBranchId()).Returns(2);
        var branchTwo = await fixture.Service.GetDocumentTypesAsync();
        Assert.Equal(new[] { 2, 1, 3 }, branchTwo.Data!.Select(t => t.Id));

        fixture.Scope.Setup(s => s.GetCurrentBranchId()).Returns(1);
        await fixture.Service.GetDocumentTypesAsync();
        fixture.Repository.Verify(r => r.Query(), Times.Exactly(3));
    }

    [Fact]
    public async Task DocumentTypes_NonHeadquartersWithoutBranch_DoesNotCountGlobalUsage()
    {
        using var fixture = new Fixture();
        fixture.Types.AddRange(new[] { Type(1, "ألف"), Type(2, "باء", Doc(branch: 1)) });
        await fixture.Service.GetDocumentTypesAsync();
        fixture.Scope.Setup(s => s.IsHQ()).Returns(false);

        await Assert.ThrowsAsync<PensionMS.Core.Exceptions.BranchScopeAccessException>(() => fixture.Service.GetDocumentTypesAsync());
        fixture.Repository.Verify(r => r.Query(), Times.Once);
    }

    private static DocumentType Type(int id, string name, params Document[] documents) => new()
    {
        DocumentTypeID = id,
        TypeName = name,
        Documents = documents.ToList()
    };

    private static Document Doc(int? branch = null, bool deleted = false) => new()
    {
        IsDeleted = deleted,
        RetirementFile = branch.HasValue ? new RetirementFile { PensionBranchID = branch.Value } : null
    };

    private sealed class Fixture : IDisposable
    {
        public List<DocumentType> Types { get; } = new();
        public Mock<IRepository<DocumentType>> Repository { get; } = new();
        public Mock<ITestBranchScope> Scope { get; } = new();
        public TestClock Clock { get; } = new();
        public MemoryCache Cache { get; }
        public LookupService Service { get; }

        public Fixture()
        {
            Cache = new MemoryCache(new MemoryCacheOptions { Clock = Clock });
            Repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<DocumentType>(Types));
            var unit = new Mock<IUnitOfWork>();
            unit.Setup(u => u.DocumentTypes).Returns(Repository.Object);
            Scope.Setup(s => s.IsHQ()).Returns(true);
            Service = new LookupService(unit.Object, TestPensionAccess.From(unit.Object, Scope.Object), Cache);
        }

        public void Dispose() => Cache.Dispose();
    }

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 2, 0, 0, 0, TimeSpan.Zero);
    }
}
