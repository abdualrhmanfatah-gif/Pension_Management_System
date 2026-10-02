using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PensionMS.Infrastructure.Data;

namespace PensionMS.API.Data;

// EF tooling must not execute application startup, seeding, or IIS setup.
public sealed class PensionDbContextFactory : IDesignTimeDbContextFactory<PensionDbContext>
{
    public PensionDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(root, "appsettings.json")))
            root = Path.Combine(root, "PensionMS.API");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var options = new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        return new PensionDbContext(options);
    }
}
