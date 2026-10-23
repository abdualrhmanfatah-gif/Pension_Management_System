using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Audit;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class AuditServiceCollectionExtensions
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ILoginHistoryService, LoginHistoryService>();
        services.AddScoped<IImmutableAuditService, ImmutableAuditService>();
        services.AddScoped<ISnapshotService, SnapshotService>();
        return services;
    }
}
