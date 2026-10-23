using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Services.Accounting;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class AccountingServiceCollectionExtensions
{
    public static IServiceCollection AddAccountingServices(this IServiceCollection services)
    {
        services.AddScoped<ICostCenterService, CostCenterService>();
        services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<IJournalEntryTypeService, JournalEntryTypeService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IVoucherService, VoucherService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
        services.AddScoped<IClosingService, ClosingService>();
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<IBankTransactionService, BankTransactionService>();
        return services;
    }
}
