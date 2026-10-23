using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Settlements;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class SettlementsServiceCollectionExtensions
{
    public static IServiceCollection AddSettlementsServices(this IServiceCollection services)
    {
        services.AddScoped<ISettlementService, SettlementService>();
        return services;
    }
}
