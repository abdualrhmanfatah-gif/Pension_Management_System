using PensionMS.Infrastructure.Reporting.Exporters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Reporting;
using PensionMS.Infrastructure.DependencyInjection;
using PensionMS.Infrastructure.Jobs;
using PensionMS.Infrastructure.Reporting;
using Xunit;

namespace PensionMS.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Registration_PreservesPersistenceAndExporterLifetimes()
    {
        var services = new ServiceCollection().AddPensionInfrastructureServices();

        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, s => s.ServiceType == typeof(IUnitOfWork)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, s => s.ServiceType == typeof(IRepository<>)).Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, s => s.ServiceType == typeof(IReportRegistry)).Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, s => s.ServiceType == typeof(ExcelReportTemplate)).Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, s => s.ServiceType == typeof(ExcelReportExporter)).Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, s => s.ServiceType == typeof(UploaderStatsReportExporter)).Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, s => s.ServiceType == typeof(PdfReportExporter)).Lifetime);
    }

    [Fact]
    public void Registration_EnablesOnlyExistingBackgroundJobsAndCitizenAdapters()
    {
        var services = new ServiceCollection().AddPensionInfrastructureServices();
        var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService)).ToArray();

        Assert.Equal(2, hostedServices.Length);
        Assert.Contains(hostedServices, s => s.ImplementationType == typeof(ExpiredPledgesJob));
        Assert.Contains(hostedServices, s => s.ImplementationType == typeof(NotificationOutboxProcessor));
        Assert.DoesNotContain(services, s => s.ImplementationType?.Name == "PledgeExpiryCheckerService");
        Assert.DoesNotContain(services, s => s.ServiceType.FullName == "PensionMS.Core.Interfaces.ICitizenAuthService"
            || s.ServiceType.FullName == "PensionMS.Core.Interfaces.ICitizenDataService"
            || s.ServiceType.FullName == "PensionMS.Core.Interfaces.ICitizenSupportService"
            || s.ServiceType.FullName == "PensionMS.Core.Interfaces.ICitizenServicePortal");
        Assert.Single(services, s => s.ServiceType == typeof(ISmsService));
        Assert.Single(services, s => s.ServiceType == typeof(IOtpService));
    }

    [Fact]
    public void Registration_ContainsNoRetiredSeparationOfDutiesServices()
    {
        var services = new ServiceCollection().AddPensionInfrastructureServices();
        Assert.DoesNotContain(services, s => s.ServiceType.Name is "ISoDGuard" or "ISoDRule");
        Assert.DoesNotContain(typeof(InfrastructureServiceCollectionExtensions).Assembly.GetTypes(),
            t => t.Namespace == "PensionMS.Infrastructure.Services.Security.SoD");
    }

    [Fact]
    public void BusinessContracts_AreDefinedInCore()
    {
        var services = new ServiceCollection().AddPensionInfrastructureServices();
        var businessContracts = services.Select(s => s.ServiceType)
            .Where(t => t.IsInterface && t.Namespace?.StartsWith("PensionMS.") == true);

        Assert.All(businessContracts, t => Assert.Equal(typeof(IUnitOfWork).Assembly, t.Assembly));
        Assert.Empty(typeof(InfrastructureServiceCollectionExtensions).Assembly.GetTypes().Where(t => t.IsInterface));
    }

    [Fact]
    public void Registration_ValidatesCompleteDependencyGraphWithoutConnectingToDatabase()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddSignalR();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IHostApplicationLifetime>());
        services.AddDbContext<PensionDbContext>(options => options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMSStructureTests;Integrated Security=True"));
        services.AddPensionInfrastructureServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    [Fact]
    public void Structure_SeparatesFileCommandsQueriesAndReports()
    {
        var services = new ServiceCollection().AddPensionInfrastructureServices();
        var commands = Assert.Single(services, s => s.ServiceType == typeof(IRetirementFileService));
        var queries = Assert.Single(services, s => s.ServiceType == typeof(IFileQueryService));
        var reports = Assert.Single(services, s => s.ServiceType == typeof(IRetirementFileReportService));

        Assert.Equal(typeof(RetirementFileService), commands.ImplementationType);
        Assert.Equal(typeof(FileQueryService), queries.ImplementationType);
        Assert.NotEqual(commands.ImplementationType, queries.ImplementationType);
        Assert.NotEqual(commands.ImplementationType, reports.ImplementationType);
        Assert.False(typeof(IFileQueryService).IsAssignableFrom(typeof(RetirementFileService)));
        Assert.DoesNotContain(typeof(RetirementFileService).Assembly.GetTypes(),
            type => type.Namespace == "PensionMS.Infrastructure.Services");
    }
}
