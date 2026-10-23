using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Tests.Services;

public class RetireeLifeStatusTests
{
    [Fact]
    public async Task UpdateAsync_CannotSetDeceasedHealthStatusWhileRetireeIsAlive()
    {
        var retiree = new Retiree { RetireeID = 42, IsAlive = true, HealthStatusID = HealthStatusConstants.Healthy };
        var repo = new Mock<IRepository<Retiree>>();
        repo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(retiree);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Retirees).Returns(repo.Object);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RetireeService(unitOfWork.Object, Mock.Of<IFileStorageService>(),
            Mock.Of<IDocumentService>(), Mock.Of<IMissingDocumentService>(),
            NullLogger<RetireeService>.Instance, cache, PensionMS.Testing.TestPensionAccess.All(unitOfWork.Object));

        var result = await service.UpdateAsync(42,
            new UpdateRetireeDto { HealthStatusID = HealthStatusConstants.Deceased }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(HealthStatusConstants.Healthy, retiree.HealthStatusID);
        unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
