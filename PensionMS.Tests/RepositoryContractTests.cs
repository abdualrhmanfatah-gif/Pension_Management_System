using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;

namespace PensionMS.Tests;

public class RepositoryContractTests
{
    [Fact]
    public void HardDeleteOfSoftDeletableRows_IsRejectedBeforeTrackerChanges()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        var first = new RetirementFile { FileID = 1, FileNumber = "test-a" };
        var second = new RetirementFile { FileID = 2, FileNumber = "test-b" };
        db.AttachRange(first, second);
        var repository = new Repository<RetirementFile>(db);
        Assert.Throws<InvalidOperationException>(() => repository.Delete(first));
        Assert.Throws<InvalidOperationException>(() => repository.DeleteRange(new[] { first, second }));
        Assert.Equal(EntityState.Unchanged, db.Entry(first).State);
        Assert.Equal(EntityState.Unchanged, db.Entry(second).State);
    }
}
