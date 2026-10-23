using Microsoft.Extensions.DependencyInjection;
using PensionMS.Infrastructure.Jobs;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class BackgroundJobsServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobsServices(this IServiceCollection services)
    {
        services.AddScoped<IExpiredPledgeProcessor, ExpiredPledgeProcessor>();
        services.AddHostedService<ExpiredPledgesJob>();
        services.AddHostedService<NotificationOutboxProcessor>();
        return services;
    }
}
