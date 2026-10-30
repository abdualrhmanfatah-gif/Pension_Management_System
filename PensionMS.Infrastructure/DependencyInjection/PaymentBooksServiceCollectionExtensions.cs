using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.PaymentBooks;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class PaymentBooksServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentBooksServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentBookService, PaymentBookService>();
        return services;
    }
}
