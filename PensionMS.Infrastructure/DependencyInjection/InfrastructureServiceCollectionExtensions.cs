using Microsoft.Extensions.DependencyInjection;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPensionInfrastructureServices(this IServiceCollection services)
    {
        services.AddSecurityServices();
        services.AddDocumentsServices();
        services.AddPersistenceServices();
        services.AddPensionServices();
        services.AddAdministrationServices();
        services.AddSettlementsServices();
        services.AddHRServices();
        services.AddPaymentBooksServices();
        services.AddPayrollServices();
        services.AddAccountingServices();
        services.AddInvestmentServices();
        services.AddCitizenServices();
        services.AddInventoryServices();
        services.AddAssetsServices();
        services.AddAuditServices();
        services.AddReportingServices();
        services.AddNotificationsServices();
        services.AddBackgroundJobsServices();
        return services;
    }
}
