using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces.Assets;
using PensionMS.Core.Interfaces.Services;
using PensionMS.Infrastructure.Services.Assets;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class AssetsServiceCollectionExtensions
{
    public static IServiceCollection AddAssetsServices(this IServiceCollection services)
    {
        services.AddScoped<IFixedAssetService, FixedAssetService>();
        services.AddScoped<IAssetStocktakeService, AssetStocktakeService>();
        return services;
    }
}
