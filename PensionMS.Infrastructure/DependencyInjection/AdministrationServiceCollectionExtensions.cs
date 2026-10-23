using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Administration;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class AdministrationServiceCollectionExtensions
{
    public static IServiceCollection AddAdministrationServices(this IServiceCollection services)
    {
        services.AddScoped<IRankService, RankService>();
        services.AddScoped<IAdminSettingsService, AdminSettingsService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ILookupService, LookupService>();
        return services;
    }
}
