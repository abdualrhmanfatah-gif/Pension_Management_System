using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Administration.Tracking;
using PensionMS.Infrastructure.Services.Pension.MinistryApproval;
using PensionMS.Infrastructure.Services.Workflow;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class PensionServiceCollectionExtensions
{
    public static IServiceCollection AddPensionServices(this IServiceCollection services)
    {
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IEntitledPersonService, EntitledPersonService>();
        services.AddScoped<IFileNoteService, FileNoteService>();
        services.AddScoped<IAchievementObstacleService, AchievementObstacleService>();
        services.AddScoped<IFileQueryService, FileQueryService>();
        services.AddScoped<IFileStatusService, FileStatusService>();
        services.AddScoped<IGuardianService, GuardianService>();
        services.AddScoped<IMinistryApprovalService, MinistryApprovalService>();
        services.AddScoped<IPensionSuspensionService, PensionSuspensionService>();
        services.AddScoped<IRetireeService, RetireeService>();
        services.AddScoped<IRetirementFileService, RetirementFileService>();
        services.AddScoped<IFileReviewService, FileReviewService>();
        services.AddScoped<IFinancialEntitlementService, FinancialEntitlementService>();
        services.AddScoped<IEntitlementEngine, EntitlementEngine>();
        services.AddScoped<IWorkflowTaskService, WorkflowTaskService>();
        return services;
    }
}
