using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Investment;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class InvestmentServiceCollectionExtensions
{
    public static IServiceCollection AddInvestmentServices(this IServiceCollection services)
    {
        services.AddScoped<IInvestmentDocumentValidationService, InvestmentDocumentValidationService>();
        services.AddScoped<IInvestmentGovernanceService, InvestmentGovernanceService>();
        services.AddScoped<IInvestmentLeasingService, InvestmentLeasingService>();
        services.AddScoped<IInvestmentLookupService, InvestmentLookupService>();
        services.AddScoped<IInvestmentMaintenanceService, InvestmentMaintenanceService>();
        services.AddScoped<IInvestmentPortfolioService, InvestmentPortfolioService>();
        services.AddScoped<IInvestmentProjectService, InvestmentProjectService>();
        services.AddScoped<IInvestmentPropertyService, InvestmentPropertyService>();
        return services;
    }
}
