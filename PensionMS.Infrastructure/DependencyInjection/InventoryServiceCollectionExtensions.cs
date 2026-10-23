using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Inventory;
using PensionMS.Infrastructure.Services.Inventory;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class InventoryServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryItemService, InventoryItemService>();
        services.AddScoped<IInventoryCustodyService, InventoryCustodyService>();
        services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
        services.AddScoped<ISimpleStocktakeService, SimpleStocktakeService>();
        return services;
    }
}
