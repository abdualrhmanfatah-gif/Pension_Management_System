using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccessiblePensionRepository, AccessiblePensionRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();
        return services;
    }
}
