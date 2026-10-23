using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.HR;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class HRServiceCollectionExtensions
{
    public static IServiceCollection AddHRServices(this IServiceCollection services)
    {
        services.AddScoped<ICorrespondenceService, CorrespondenceService>();
        services.AddScoped<IHrAdministrativeOrderService, HrAdministrativeOrderService>();
        services.AddScoped<IHrAttendanceService, HrAttendanceService>();
        services.AddScoped<IHrDocumentService, HrDocumentService>();
        services.AddScoped<IHrEmployeeService, HrEmployeeService>();
        services.AddScoped<IHrLeaveRequestService, HrLeaveRequestService>();
        services.AddScoped<IHrReferenceDataService, HrReferenceDataService>();
        services.AddScoped<IHrServiceHistoryService, HrServiceHistoryService>();
        return services;
    }
}
