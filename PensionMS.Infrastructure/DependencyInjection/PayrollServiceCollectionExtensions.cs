using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Payroll;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class PayrollServiceCollectionExtensions
{
    public static IServiceCollection AddPayrollServices(this IServiceCollection services)
    {
        services.AddScoped<IPayrollGenerationService, PayrollGenerationService>();
        services.AddScoped<ISalaryPaymentBatchService, SalaryPaymentBatchService>();
        services.AddScoped<ISalaryRecipientService, SalaryRecipientService>();
        return services;
    }
}
