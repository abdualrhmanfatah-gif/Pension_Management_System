using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Core.Reporting;
using PensionMS.Infrastructure.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class ReportingServiceCollectionExtensions
{
    public static IServiceCollection AddReportingServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDiscrepancyService, DiscrepancyService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IReportRegistry, ReportRegistry>();
        services.AddSingleton<ExcelReportTemplate>();
        services.AddSingleton<ExcelReportExporter>();
        services.AddSingleton<UploaderStatsReportExporter>();
        services.AddSingleton<PdfReportExporter>();
        services.AddScoped<IReportDataService, ReportDataService>();
        services.AddScoped<IAccountingExportService, AccountingExportService>();
        services.AddScoped<IRetirementFileReportService, RetirementFileReportService>();
        services.AddSingleton<RetirementFileReportExporter>();
        return services;
    }
}
