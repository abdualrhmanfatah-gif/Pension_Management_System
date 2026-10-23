using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Notifications;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class NotificationsServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminNotificationService, AdminNotificationService>();
        return services;
    }
}
