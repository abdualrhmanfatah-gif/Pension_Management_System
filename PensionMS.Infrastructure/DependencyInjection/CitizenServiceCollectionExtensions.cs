using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Citizen;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class CitizenServiceCollectionExtensions
{
    public static IServiceCollection AddCitizenServices(this IServiceCollection services)
    {
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IOtpService, OtpService>();
        return services;
    }
}
