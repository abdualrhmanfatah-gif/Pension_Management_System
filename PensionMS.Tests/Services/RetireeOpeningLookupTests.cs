using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetireeOpeningLookupTests
{
    [Fact]
    public async Task BR07_LookupIncludesUnfiledAndOtherBranchRetireesButHidesTheirFiles()
    {
        var unit = new Mock<IUnitOfWork>();
        var unfiled = new Retiree { RetireeID = 1, NationalID = "lookup-1", MilitaryNumber = "101",
            ApprovalItem = new MinistryApprovalItem { IsProcessed = true } };
        var foreign = new Retiree { RetireeID = 2, NationalID = "lookup-2", MilitaryNumber = "102",
            ApprovalItem = new MinistryApprovalItem { IsProcessed = true } };
        var file = new RetirementFile { FileID = 22, RetireeID = 2, PensionBranchID = 2 };
        foreign.RetirementFiles.Add(file);
        var retirees = new Mock<IRepository<Retiree>>();
        retirees.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Retiree>([unfiled, foreign]));
        unit.Setup(u => u.Retirees).Returns(retirees.Object);
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([file]));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(i => i.GetCurrentBranchId()).Returns(1);
        var access = TestPensionAccess.From(unit.Object, identity.Object);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RetireeService(unit.Object, Mock.Of<IFileStorageService>(), Mock.Of<IDocumentService>(),
            Mock.Of<IMissingDocumentService>(), NullLogger<RetireeService>.Instance, cache, access);

        var list = await service.GetAllAsync(1, 20);
        Assert.True(list.IsSuccess);
        Assert.Equal(2, list.Data!.Items.Count);
        Assert.All(list.Data.Items, r => { Assert.Equal(0, r.FileCount); Assert.Null(r.FileID); });
        Assert.True((await service.GetByIdAsync(1)).IsSuccess);
        Assert.True((await service.GetByNationalIDAsync("lookup-2")).IsSuccess);
        Assert.True((await service.GetByMilitaryNumberAsync("102")).IsSuccess);
        Assert.True((await service.GetFullDataAsync(1)).IsSuccess);
        Assert.True((await service.GetFullDataAsync(2)).IsSuccess);
        var scoped = await access.ForCurrentOperationAsync();
        Assert.Empty(scoped.Files());
        Assert.False(await scoped.CanEditRetireeAsync(2));
    }
}
