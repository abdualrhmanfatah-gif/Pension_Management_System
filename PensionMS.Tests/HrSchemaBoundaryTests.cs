using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Tests;

public class HrSchemaBoundaryTests
{
    private readonly PensionDbContext _dbContext;

    public HrSchemaBoundaryTests()
    {
        var options = new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HrSchemaBoundaryTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        _dbContext = new PensionDbContext(options);
    }

    [Fact]
    public void HrEntities_UseNoPrefix()
    {
        var hrTableNames = _dbContext.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == "PensionMS.Core.Entities.HR")
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => tableName is not null)
            .Cast<string>()
            .ToList();

        Assert.NotEmpty(hrTableNames);
        Assert.All(hrTableNames, tableName => Assert.DoesNotContain("_", tableName, StringComparison.Ordinal));
    }

    [Fact]
    public void HrEntities_DoNotReferencePensionAggregates()
    {
        var blockedPrincipalTypes = new HashSet<Type>
        {
            typeof(Retiree),
            typeof(RetirementFile),
            typeof(MinistryApprovalBatch),
            typeof(MinistryApprovalItem)
        };

        var violations = _dbContext.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == "PensionMS.Core.Entities.HR")
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Where(foreignKey => blockedPrincipalTypes.Contains(foreignKey.PrincipalEntityType.ClrType))
            .Select(foreignKey => $"{foreignKey.DeclaringEntityType.ClrType.Name} -> {foreignKey.PrincipalEntityType.ClrType.Name}")
            .ToList();

        Assert.True(violations.Count == 0, $"HR boundary violation(s): {string.Join(", ", violations)}");
    }
}
