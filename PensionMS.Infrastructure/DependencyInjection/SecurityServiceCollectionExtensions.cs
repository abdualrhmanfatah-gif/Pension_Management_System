using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEncryptionService, Aes256EncryptionService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IComplianceService, ComplianceService>();
        services.AddScoped<IRiskScoringService, RiskScoringService>();
        services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
        return services;
    }
}
