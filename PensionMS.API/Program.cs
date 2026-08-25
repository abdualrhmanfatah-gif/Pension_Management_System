using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using PensionMS.API.Middleware;
using PensionMS.API.Security;
using PensionMS.Core.Constants;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Interfaces.Assets;
using PensionMS.Core.Interfaces.Inventory;
using PensionMS.Core.Interfaces.Services;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services;
using PensionMS.Infrastructure.Services.Accounting;
using PensionMS.Core.Reporting;
using PensionMS.Infrastructure.Reporting;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text;
using PensionMS.API.Interceptors;
using PensionMS.Infrastructure.Jobs;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════
// KESTREL — زيادة مهلة الرفع إلى 5 دقائق
// ═══════════════════════════════════════
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(6);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});

// ═══════════════════════════════════════
// EPPlus LICENSE
// ═══════════════════════════════════════
ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

// ═══════════════════════════════════════
// DATABASE
// ═══════════════════════════════════════
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<PensionDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PensionMS.Infrastructure")
              .UseCompatibilityLevel(120)
              .CommandTimeout(30)

    ).AddInterceptors(interceptor);
});

// ═══════════════════════════════════════
// DEPENDENCY INJECTION
// ═══════════════════════════════════════
// ═══ Core & Infrastructure ═══
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEncryptionService, Aes256EncryptionService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBranchScopeProvider, BranchScopeProvider>();

// ═══ Retirement & Files ═══
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentExemptionService, DocumentExemptionService>();
builder.Services.AddScoped<IDocumentPledgeService, DocumentPledgeService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IEntitledPersonService, EntitledPersonService>();
builder.Services.AddScoped<IFileNoteService, FileNoteService>();
builder.Services.AddScoped<IAchievementObstacleService, AchievementObstacleService>();
builder.Services.AddScoped<IFileQueryService, RetirementFileService>();
builder.Services.AddScoped<IFileStatusService, FileStatusService>();
builder.Services.AddScoped<IGuardianService, GuardianService>();
builder.Services.AddScoped<IMinistryApprovalService, MinistryApprovalService>();
builder.Services.AddScoped<IMissingDocumentService, MissingDocumentService>();
builder.Services.AddScoped<IPensionSuspensionService, PensionSuspensionService>();
builder.Services.AddScoped<IRankService, RankService>();
builder.Services.AddScoped<IRetireeService, RetireeService>();
builder.Services.AddScoped<IRetirementFileService, RetirementFileService>();
builder.Services.AddScoped<IFileReviewService, FileReviewService>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<ISettlementService, SettlementService>();

// ═══ HR ═══
builder.Services.AddScoped<ICorrespondenceService, PensionMS.Infrastructure.Services.HR.CorrespondenceService>();
builder.Services.AddScoped<IHrAdministrativeOrderService, PensionMS.Infrastructure.Services.HR.HrAdministrativeOrderService>();
builder.Services.AddScoped<IHrAttendanceService, PensionMS.Infrastructure.Services.HR.HrAttendanceService>();
builder.Services.AddScoped<IHrDocumentService, PensionMS.Infrastructure.Services.HR.HrDocumentService>();
builder.Services.AddScoped<IHrEmployeeService, PensionMS.Infrastructure.Services.HR.HrEmployeeService>();
builder.Services.AddScoped<IHrLeaveRequestService, PensionMS.Infrastructure.Services.HR.HrLeaveRequestService>();
builder.Services.AddScoped<IHrReferenceDataService, PensionMS.Infrastructure.Services.HR.HrReferenceDataService>();
builder.Services.AddScoped<IHrServiceHistoryService, PensionMS.Infrastructure.Services.HR.HrServiceHistoryService>();

// ═══ Financial & Budget ═══
builder.Services.AddScoped<IFinancialEntitlementService, FinancialEntitlementService>();
builder.Services.AddScoped<IPaymentBookService, PaymentBookService>();
builder.Services.AddScoped<IPayrollGenerationService, PayrollGenerationService>();
builder.Services.AddScoped<ISalaryPaymentBatchService, SalaryPaymentBatchService>();
builder.Services.AddScoped<ISalaryRecipientService, SalaryRecipientService>();

// ═══ Accounting ═══
builder.Services.AddScoped<ICostCenterService, PensionMS.Infrastructure.Services.Accounting.CostCenterService>();
builder.Services.AddScoped<IChartOfAccountService, PensionMS.Infrastructure.Services.Accounting.ChartOfAccountService>();
builder.Services.AddScoped<IJournalEntryService, PensionMS.Infrastructure.Services.Accounting.JournalEntryService>();
builder.Services.AddScoped<IJournalEntryTypeService, PensionMS.Infrastructure.Services.Accounting.JournalEntryTypeService>();
builder.Services.AddScoped<IReportingService, PensionMS.Infrastructure.Services.Accounting.ReportingService>();
builder.Services.AddScoped<IVoucherService, PensionMS.Infrastructure.Services.Accounting.VoucherService>();
builder.Services.AddScoped<IBudgetService, PensionMS.Infrastructure.Services.Accounting.BudgetService>();
builder.Services.AddScoped<IFiscalPeriodService, PensionMS.Infrastructure.Services.Accounting.FiscalPeriodService>();
builder.Services.AddScoped<IClosingService, PensionMS.Infrastructure.Services.Accounting.ClosingService>();


// ═══ Investment ═══
builder.Services.AddScoped<IInvestmentDocumentValidationService, InvestmentDocumentValidationService>();
builder.Services.AddScoped<IInvestmentGovernanceService, PensionMS.Infrastructure.Services.Investment.InvestmentGovernanceService>();
builder.Services.AddScoped<IInvestmentLeasingService, PensionMS.Infrastructure.Services.Investment.InvestmentLeasingService>();
builder.Services.AddScoped<IInvestmentLookupService, PensionMS.Infrastructure.Services.Investment.InvestmentLookupService>();
builder.Services.AddScoped<IInvestmentMaintenanceService, PensionMS.Infrastructure.Services.Investment.InvestmentMaintenanceService>();
builder.Services.AddScoped<IInvestmentPortfolioService, PensionMS.Infrastructure.Services.Investment.InvestmentPortfolioService>();
builder.Services.AddScoped<IInvestmentProjectService, PensionMS.Infrastructure.Services.Investment.InvestmentProjectService>();
builder.Services.AddScoped<IInvestmentPropertyService, PensionMS.Infrastructure.Services.Investment.InvestmentPropertyService>();

// ═══ Citizen Portal ═══
builder.Services.AddScoped<ISmsService, PensionMS.Infrastructure.Services.Citizen.SmsService>();
builder.Services.AddScoped<IOtpService, PensionMS.Infrastructure.Services.Citizen.OtpService>();

// ═══ Inventory ═══
builder.Services.AddScoped<IInventoryItemService, PensionMS.Infrastructure.Services.Inventory.InventoryItemService>();
builder.Services.AddScoped<IInventoryCustodyService, PensionMS.Infrastructure.Services.Inventory.InventoryCustodyService>();
builder.Services.AddScoped<IInventoryTransactionService, PensionMS.Infrastructure.Services.Inventory.InventoryTransactionService>();
builder.Services.AddScoped<ISimpleStocktakeService, PensionMS.Infrastructure.Services.Inventory.SimpleStocktakeService>();

// ═══ Fixed Assets ═══
builder.Services.AddScoped<IFixedAssetService, PensionMS.Infrastructure.Services.Assets.FixedAssetService>();
builder.Services.AddScoped<IAssetStocktakeService, PensionMS.Infrastructure.Services.Assets.AssetStocktakeService>();

// ═══ System & Admin ═══
builder.Services.AddScoped<IAdminSettingsService, AdminSettingsService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IBankTransactionService, BankTransactionService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDiscrepancyService, DiscrepancyService>();
builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddSingleton<IReportRegistry, ReportRegistry>();
builder.Services.AddSingleton<ExcelReportExporter>();
builder.Services.AddSingleton<PdfReportExporter>();
builder.Services.AddScoped<IReportDataService, ReportDataService>();
builder.Services.AddScoped<IAccountingExportService, AccountingExportService>();
builder.Services.AddScoped<IUserService, UserService>();

// ═══ Workflow & Compliance ═══
builder.Services.AddScoped<IComplianceEngine, ComplianceEngine>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IEntitlementEngine, EntitlementEngine>();
builder.Services.AddScoped<IImmutableAuditService, ImmutableAuditService>();
builder.Services.AddScoped<IRiskScoringService, RiskScoringService>();
builder.Services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
builder.Services.AddScoped<ISnapshotService, SnapshotService>();
builder.Services.AddScoped<ISoDGuard, SoDGuardService>();
builder.Services.AddScoped<ISoDRule, PensionMS.Infrastructure.Services.SoDRules.ApproverCannotActivateRule>();
builder.Services.AddScoped<ISoDRule, PensionMS.Infrastructure.Services.SoDRules.DocumentUploaderCannotApproveRule>();
builder.Services.AddScoped<ISoDRule, PensionMS.Infrastructure.Services.SoDRules.FinancialEditorCannotApproveRule>();
builder.Services.AddScoped<ISoDRule, PensionMS.Infrastructure.Services.SoDRules.OverrideSoDRule>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();


builder.Services.AddHostedService<ExpiredPledgesJob>();
builder.Services.AddHostedService<NotificationOutboxProcessor>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// تسجيل المعالج المخصص للصلاحيات
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// ═══════════════════════════════════════
// JWT AUTHENTICATION
// ═══════════════════════════════════════
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
                context.Response.Headers.Append("X-Token-Expired", "true");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            // Prevent default 401 HTML response — return JSON instead
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    isSuccess = false,
                    message = "انتهت صلاحية الجلسة أو التوكن غير صالح. يرجى تسجيل الدخول مرة أخرى.",
                    statusCode = 401
                }));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    isSuccess = false,
                    message = "ليس لديك صلاحية لتنفيذ هذا الإجراء.",
                    statusCode = 403
                }));
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // ═══ Policy-Based Authorization — كل سياسة مرتبطة بصلاحية دقيقة ═══
    // استخدام PermissionRequirement للتحقق المباشر من قاعدة البيانات

    // ── File ──
    options.AddPolicy("File.ChangeStatus.Approve",  p => p.AddRequirements(new PermissionRequirement("File.ChangeStatus.Approve")));
    options.AddPolicy("File.ChangeStatus.Return",   p => p.AddRequirements(new PermissionRequirement("File.ChangeStatus.Return")));
    options.AddPolicy("File.Create",               p => p.AddRequirements(new PermissionRequirement("File.Create")));
    options.AddPolicy("File.Delete.Soft",           p => p.AddRequirements(new PermissionRequirement("File.Delete.Soft")));
    options.AddPolicy("File.Documents.Upload",      p => p.AddRequirements(new PermissionRequirement("File.Documents.Upload")));
    options.AddPolicy("File.Documents.Update",      p => p.AddRequirements(new PermissionRequirement("File.Documents.Update")));
    options.AddPolicy("File.Edit.BasicData",        p => p.AddRequirements(new PermissionRequirement("File.Edit.BasicData")));
    options.AddPolicy("File.Edit.FinancialData",    p => p.AddRequirements(new PermissionRequirement("File.Edit.FinancialData")));
    options.AddPolicy("File.Override",              p => p.AddRequirements(new PermissionRequirement("File.Override")));
    options.AddPolicy("File.View",                  p => p.AddRequirements(new PermissionRequirement("File.View")));
    options.AddPolicy("File.View.BankData",         p => p.AddRequirements(new PermissionRequirement("File.View.BankData")));
    options.AddPolicy("File.View.Sensitive",        p => p.AddRequirements(new PermissionRequirement("File.View.Sensitive")));

    // ── Unified Policies for Common Tasks ──
    options.AddPolicy("File.View.History",          p => p.AddRequirements(new PermissionRequirement("File.View.History")));
    options.AddPolicy("File.Review.View",           p => p.AddRequirements(new PermissionRequirement("File.Review.View")));
    options.AddPolicy("File.Review.Approve",        p => p.AddRequirements(new PermissionRequirement("File.Review.Approve")));
    options.AddPolicy("File.Review.Reject",         p => p.AddRequirements(new PermissionRequirement("File.Review.Reject")));

    // ── Export ──
    options.AddPolicy("Export.Financial",            p => p.AddRequirements(new PermissionRequirement("Export.Financial")));
    options.AddPolicy("Export.Sensitive",            p => p.AddRequirements(new PermissionRequirement("Export.Sensitive")));

    // ── EntitledPerson ──
    options.AddPolicy("EntitledPerson.Create",      p => p.AddRequirements(new PermissionRequirement("EntitledPerson.Create")));
    options.AddPolicy("EntitledPerson.Delete",      p => p.AddRequirements(new PermissionRequirement("EntitledPerson.Delete")));
    options.AddPolicy("EntitledPerson.Edit",        p => p.AddRequirements(new PermissionRequirement("EntitledPerson.Edit")));
    options.AddPolicy("EntitledPerson.Lifecycle",   p => p.AddRequirements(new PermissionRequirement("EntitledPerson.Lifecycle")));
    options.AddPolicy("EntitledPerson.View",        p => p.AddRequirements(new PermissionRequirement("EntitledPerson.View")));

    // ── Guardian ──
    options.AddPolicy("Guardian.Add",               p => p.AddRequirements(new PermissionRequirement("Guardian.Add")));
    options.AddPolicy("Guardian.Delete",             p => p.AddRequirements(new PermissionRequirement("Guardian.Delete")));
    options.AddPolicy("Guardian.Edit",              p => p.AddRequirements(new PermissionRequirement("Guardian.Edit")));
    options.AddPolicy("Guardian.Manage",            p => p.AddRequirements(new PermissionRequirement("Guardian.Manage")));
    options.AddPolicy("Guardian.View",              p => p.AddRequirements(new PermissionRequirement("Guardian.View")));

    // ── Retiree ──
    options.AddPolicy(PermissionConstants.RetireeDelete, p => p.AddRequirements(new PermissionRequirement(PermissionConstants.RetireeDelete)));
    options.AddPolicy(PermissionConstants.RetireeEdit,   p => p.AddRequirements(new PermissionRequirement(PermissionConstants.RetireeEdit)));
    options.AddPolicy(PermissionConstants.RetireeView,   p => p.AddRequirements(new PermissionRequirement(PermissionConstants.RetireeView)));

    // ── Agent ──
    options.AddPolicy("Agent.Add",                  p => p.AddRequirements(new PermissionRequirement("Agent.Add")));
    options.AddPolicy("Agent.Edit",                 p => p.AddRequirements(new PermissionRequirement("Agent.Edit")));
    options.AddPolicy("Agent.Manage",               p => p.AddRequirements(new PermissionRequirement("Agent.Manage")));
    options.AddPolicy("Agent.View",                 p => p.AddRequirements(new PermissionRequirement("Agent.View")));

    // ── FileNote ──
    options.AddPolicy("FileNote.Edit",              p => p.AddRequirements(new PermissionRequirement("FileNote.Edit")));
    options.AddPolicy("FileNote.Manage",            p => p.AddRequirements(new PermissionRequirement("FileNote.Manage")));
    options.AddPolicy("FileNote.View",              p => p.AddRequirements(new PermissionRequirement("FileNote.View")));

    // ── AchievementObstacle ──
    options.AddPolicy("AchievementObstacle.View",   p => p.AddRequirements(new PermissionRequirement("AchievementObstacle.View")));
    options.AddPolicy("AchievementObstacle.Manage", p => p.AddRequirements(new PermissionRequirement("AchievementObstacle.Manage")));
    options.AddPolicy("AchievementObstacle.Add",    p => p.AddRequirements(new PermissionRequirement("AchievementObstacle.Add")));
    options.AddPolicy("AchievementObstacle.Edit",   p => p.AddRequirements(new PermissionRequirement("AchievementObstacle.Edit")));
    options.AddPolicy("AchievementObstacle.Delete", p => p.AddRequirements(new PermissionRequirement("AchievementObstacle.Delete")));

    // ── DocumentExemption ──
    options.AddPolicy("DocumentExemption.Add",      p => p.AddRequirements(new PermissionRequirement("DocumentExemption.Add")));
    options.AddPolicy("DocumentExemption.Delete",   p => p.AddRequirements(new PermissionRequirement("DocumentExemption.Delete")));
    options.AddPolicy("DocumentExemption.Edit",     p => p.AddRequirements(new PermissionRequirement("DocumentExemption.Edit")));
    options.AddPolicy("DocumentExemption.Manage",   p => p.AddRequirements(new PermissionRequirement("DocumentExemption.Manage")));
    options.AddPolicy("DocumentExemption.View",     p => p.AddRequirements(new PermissionRequirement("DocumentExemption.View")));

    // ── DocumentPledge ──
    options.AddPolicy("DocumentPledge.Add",         p => p.AddRequirements(new PermissionRequirement("DocumentPledge.Add")));
    options.AddPolicy("DocumentPledge.Delete",      p => p.AddRequirements(new PermissionRequirement("DocumentPledge.Delete")));
    options.AddPolicy("DocumentPledge.Edit",        p => p.AddRequirements(new PermissionRequirement("DocumentPledge.Edit")));
    options.AddPolicy("DocumentPledge.Manage",      p => p.AddRequirements(new PermissionRequirement("DocumentPledge.Manage")));
    options.AddPolicy("DocumentPledge.View",        p => p.AddRequirements(new PermissionRequirement("DocumentPledge.View")));

    // ── MissingDocument ──
    options.AddPolicy("MissingDocument.Add",        p => p.AddRequirements(new PermissionRequirement("MissingDocument.Add")));
    options.AddPolicy("MissingDocument.Delete",     p => p.AddRequirements(new PermissionRequirement("MissingDocument.Delete")));
    options.AddPolicy("MissingDocument.Edit",       p => p.AddRequirements(new PermissionRequirement("MissingDocument.Edit")));
    options.AddPolicy("MissingDocument.Manage",     p => p.AddRequirements(new PermissionRequirement("MissingDocument.Manage")));
    options.AddPolicy("MissingDocument.View",       p => p.AddRequirements(new PermissionRequirement("MissingDocument.View")));

    // ── PaymentBook ──
    options.AddPolicy("PaymentBook.Add",            p => p.AddRequirements(new PermissionRequirement("PaymentBook.Add")));
    options.AddPolicy("PaymentBook.Edit",           p => p.AddRequirements(new PermissionRequirement("PaymentBook.Edit")));
    options.AddPolicy("PaymentBook.Manage",         p => p.AddRequirements(new PermissionRequirement("PaymentBook.Manage")));
    options.AddPolicy("PaymentBook.View",           p => p.AddRequirements(new PermissionRequirement("PaymentBook.View")));

    // ── Ministry ──
    options.AddPolicy("Ministry.Edit",              p => p.AddRequirements(new PermissionRequirement("Ministry.Edit")));
    options.AddPolicy("Ministry.Manage",            p => p.AddRequirements(new PermissionRequirement("Ministry.Manage")));
    options.AddPolicy("Ministry.View",              p => p.AddRequirements(new PermissionRequirement("Ministry.View")));

    // ── Suspension ──
    options.AddPolicy("Suspension.Add",             p => p.AddRequirements(new PermissionRequirement("Suspension.Add")));
    options.AddPolicy("Suspension.Edit",            p => p.AddRequirements(new PermissionRequirement("Suspension.Edit")));
    options.AddPolicy("Suspension.Manage",          p => p.AddRequirements(new PermissionRequirement("Suspension.Manage")));
    options.AddPolicy("Suspension.View",            p => p.AddRequirements(new PermissionRequirement("Suspension.View")));

    // ── Dashboard ──
    options.AddPolicy("Dashboard.View",             p => p.AddRequirements(new PermissionRequirement("Dashboard.View")));

    // ── SalaryRecipient ──
    options.AddPolicy("SalaryRecipient.Add",        p => p.AddRequirements(new PermissionRequirement("SalaryRecipient.Add")));
    options.AddPolicy("SalaryRecipient.Edit",       p => p.AddRequirements(new PermissionRequirement("SalaryRecipient.Edit")));
    options.AddPolicy("SalaryRecipient.Manage",     p => p.AddRequirements(new PermissionRequirement("SalaryRecipient.Manage")));
    options.AddPolicy("SalaryRecipient.View",       p => p.AddRequirements(new PermissionRequirement("SalaryRecipient.View")));

    // ── Settlement ──
    options.AddPolicy("Settlement.Edit",            p => p.AddRequirements(new PermissionRequirement("Settlement.Edit")));
    options.AddPolicy("Settlement.Manage",          p => p.AddRequirements(new PermissionRequirement("Settlement.Manage")));
    options.AddPolicy("Settlement.View",            p => p.AddRequirements(new PermissionRequirement("Settlement.View")));

    // ── SalaryBatch ──
    options.AddPolicy("SalaryBatch.Approve",        p => p.AddRequirements(new PermissionRequirement("SalaryBatch.Approve")));
    options.AddPolicy("SalaryBatch.Export",         p => p.AddRequirements(new PermissionRequirement("SalaryBatch.Export")));
    options.AddPolicy("SalaryBatch.ImportResults",  p => p.AddRequirements(new PermissionRequirement("SalaryBatch.ImportResults")));
    options.AddPolicy("SalaryBatch.Manage",         p => p.AddRequirements(new PermissionRequirement("SalaryBatch.Manage")));
    options.AddPolicy("SalaryBatch.View",           p => p.AddRequirements(new PermissionRequirement("SalaryBatch.View")));

    // ── Accounting ──
    options.AddPolicy("Accounting.View",            p => p.AddRequirements(new PermissionRequirement("Accounting.View")));
    options.AddPolicy("Accounting.Manage",          p => p.AddRequirements(new PermissionRequirement("Accounting.Manage")));
    options.AddPolicy("Accounting.Reports",         p => p.AddRequirements(new PermissionRequirement("Accounting.Reports")));
    options.AddPolicy("Accounting.CloseYear",       p => p.RequireRole("Admin", "AccountingManager"));

    // ── Inventory ──
    options.AddPolicy("Inventory.Item.View",        p => p.AddRequirements(new PermissionRequirement("Inventory.Item.View")));
    options.AddPolicy("Inventory.Item.Manage",      p => p.AddRequirements(new PermissionRequirement("Inventory.Item.Manage")));
    options.AddPolicy("Inventory.Custody.View",     p => p.AddRequirements(new PermissionRequirement("Inventory.Custody.View")));
    options.AddPolicy("Inventory.Custody.Manage",   p => p.AddRequirements(new PermissionRequirement("Inventory.Custody.Manage")));
    options.AddPolicy("Inventory.Transaction.View", p => p.AddRequirements(new PermissionRequirement("Inventory.Transaction.View")));
    options.AddPolicy("Inventory.View",             p => p.AddRequirements(new PermissionRequirement("Inventory.Item.View")));
    options.AddPolicy("Inventory.Stocktake",        p => p.AddRequirements(new PermissionRequirement("Inventory.Stocktake")));

    // ── Investment ──
    options.AddPolicy("Investment.Approve",         p => p.AddRequirements(new PermissionRequirement("Investment.Approve")));
    options.AddPolicy("Investment.Finance",         p => p.AddRequirements(new PermissionRequirement("Investment.Finance")));
    options.AddPolicy("Investment.Manage",          p => p.AddRequirements(new PermissionRequirement("Investment.Manage")));
    options.AddPolicy("Investment.View",            p => p.AddRequirements(new PermissionRequirement("Investment.View")));

    // ── HR ──
    options.AddPolicy("HR.Dashboard.View",          p => p.AddRequirements(new PermissionRequirement("HR.Dashboard.View")));
    options.AddPolicy("HR.Employee.Create",         p => p.AddRequirements(new PermissionRequirement("HR.Employee.Create")));
    options.AddPolicy("HR.Employee.Delete",         p => p.AddRequirements(new PermissionRequirement("HR.Employee.Delete")));
    options.AddPolicy("HR.Employee.Edit",           p => p.AddRequirements(new PermissionRequirement("HR.Employee.Edit")));
    options.AddPolicy("HR.Employee.View",           p => p.AddRequirements(new PermissionRequirement("HR.Employee.View")));
    options.AddPolicy("HR.Reference.Manage",        p => p.AddRequirements(new PermissionRequirement("HR.Reference.Manage")));
    options.AddPolicy("HR.Reference.View",          p => p.AddRequirements(new PermissionRequirement("HR.Reference.View")));
    options.AddPolicy("HR.ServiceHistory.View",     p => p.AddRequirements(new PermissionRequirement("HR.ServiceHistory.View")));

    // ── Correspondence ──
    options.AddPolicy("Correspondence.View",        p => p.AddRequirements(new PermissionRequirement("Correspondence.View")));
    options.AddPolicy("Correspondence.Create",      p => p.AddRequirements(new PermissionRequirement("Correspondence.Create")));
    options.AddPolicy("Correspondence.Edit",        p => p.AddRequirements(new PermissionRequirement("Correspondence.Edit")));
    options.AddPolicy("Correspondence.Delete",      p => p.AddRequirements(new PermissionRequirement("Correspondence.Delete")));

 
    // ── Audit & Admin ──
    options.AddPolicy("Audit.ViewAll",              p => p.AddRequirements(new PermissionRequirement("Audit.ViewAll")));
    options.AddPolicy("System.Admin",               p => p.AddRequirements(new PermissionRequirement("System.Admin")));
    options.AddPolicy("System.ManageUsers",         p => p.AddRequirements(new PermissionRequirement("System.ManageUsers")));
    options.AddPolicy("System.Settings",            p => p.AddRequirements(new PermissionRequirement("System.Settings")));

    // ── Report ──
    options.AddPolicy("Report.Pension.View",        p => p.AddRequirements(new PermissionRequirement("Report.Pension.View")));
    options.AddPolicy("Report.Financial.View",      p => p.AddRequirements(new PermissionRequirement("Report.Financial.View")));
    options.AddPolicy("Report.Entitlements.View",   p => p.AddRequirements(new PermissionRequirement("Report.Entitlements.View")));
    options.AddPolicy("Report.Documents.View",      p => p.AddRequirements(new PermissionRequirement("Report.Documents.View")));
    options.AddPolicy("Report.Export",              p => p.AddRequirements(new PermissionRequirement("Report.Export")));
    options.AddPolicy("Report.Admin.View",          p => p.AddRequirements(new PermissionRequirement("Report.Admin.View")));

    // ── Granular Micro-Permissions (Batch Registration) ──
    var granularPermissions = new[] {
        "Agent.Delete",
        "FileNote.Add", "FileNote.Delete",
        "AchievementObstacle.View", "AchievementObstacle.Manage",
        "AchievementObstacle.Add", "AchievementObstacle.Edit", "AchievementObstacle.Delete",
        "Investment.Add", "Investment.Delete", "Investment.Edit",
        "Ministry.Add", "Ministry.Delete", 
        "OperationalBudget.Add", "OperationalBudget.Delete", "OperationalBudget.Edit",
        "PaymentBook.Delete", "Payroll.ExcludeBeneficiary", "Payroll.ManageDeductions",
        "SalaryRecipient.Delete", "Settlement.Add", "Settlement.Delete", "Suspension.Delete",
        "System.Settings.Banks", "System.Settings.DocumentTypes", 
        "System.Settings.InvestmentTypes", "System.Settings.JobTitles",

        // ── Inventory ──
        "Inventory.Item.View", "Inventory.Item.Create", "Inventory.Item.Edit", "Inventory.Item.Delete",
        "Inventory.Custody.View", "Inventory.Custody.Create", "Inventory.Custody.Return",
        "Inventory.Transaction.View", "Inventory.Transaction.Create",
        "Inventory.Stocktake"
    };

    foreach (var permission in granularPermissions)
    {
        options.AddPolicy(permission, p => p.AddRequirements(new PermissionRequirement(permission)));
    }
});

// ═══════════════════════════════════════
// CORS
// ═══════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost") ||
            origin.StartsWith("http://127.0.0.1") ||
            origin.StartsWith("http://192.168.") ||
            origin.StartsWith("http://10.") ||
            origin.EndsWith(".pension.local") ||
            origin == "http://pension.pension.local" ||
            origin == "http://pension.local" ||
            origin == "http://192.168.1.10")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-Token-Expired");
    });
});

// ═══════════════════════════════════════
// SIGNALR
// ═══════════════════════════════════════
builder.Services.AddSignalR();

// ═══════════════════════════════════════
// RATE LIMITING
// ═══════════════════════════════════════
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("SensitiveEndpoints", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("CloseYearEndpoint", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// ═══════════════════════════════════════
// RESPONSE COMPRESSION
// ═══════════════════════════════════════
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/json; charset=utf-8" }
    );
});

// ═══════════════════════════════════════
// CONTROLLERS
// ═══════════════════════════════════════
builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new PensionMS.API.Helpers.RemoteFileModelBinderProvider());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    // Ensure dates are serialized in ISO 8601 format with UTC indicator if Kind is Utc
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// ═══════════════════════════════════════
// SWAGGER
// ═══════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pension Management System API",
        Version = "v1",
        Description = "الإدارة العامة لصندوق التقاعد الأمني"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "أدخل رمز JWT: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

// ═══════════════════════════════════════
// APPLY MIGRATIONS + SEED ADMIN USER (with retry for transient failures)
// ═══════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PensionDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Ensure IIS APPPOOL\PensionAPI has database access (login + db user)
    try
    {
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
        using var adminConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
        await adminConn.OpenAsync();
        using var cmd = adminConn.CreateCommand();
        cmd.CommandText = @"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'IIS APPPOOL\PensionAPI')
    CREATE LOGIN [IIS APPPOOL\PensionAPI] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'IIS APPPOOL\PensionAPI')
BEGIN
    CREATE USER [IIS APPPOOL\PensionAPI] FOR LOGIN [IIS APPPOOL\PensionAPI];
    ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\PensionAPI];
END";
        await cmd.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning("⚠️ تعذر التأكد من صلاحية IIS APPPOOL\\PensionAPI: {Message}", ex.Message);
    }

    for (int attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            if (attempt > 1)
            {
                logger.LogInformation("⚠️ محاولة تهيئة قاعدة البيانات رقم {Attempt}/3", attempt);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            }

            try
            {
                db.Database.Migrate();
                logger.LogInformation("✅ تم تحديث قاعدة البيانات بنجاح");
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                logger.LogWarning("⚠️ قاعدة البيانات تحتوي بالفعل على جداول؛ سيتم الاستمرار في عملية الـ Seed فقط.");
            }
            catch (Exception ex)
            {
                logger.LogWarning("⚠️ تنبيه أثناء Migration: {Message}", ex.Message);
            }

            await PensionMS.Infrastructure.Data.Seeders.PrimaryDataSeeder.SeedAllAsync(db, logger);

            break; // success
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ محاولة {Attempt}/3 فشلت: {Message}", attempt, ex.Message);
            if (attempt == 3)
                logger.LogError(ex, "❌ فشلت جميع محاولات تهيئة قاعدة البيانات");
        }
    }
}

// ═══════════════════════════════════════
// MIDDLEWARE PIPELINE
// ═══════════════════════════════════════
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{

}
app.UseResponseCompression();
app.UseCors("AllowFrontend");
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var uploadsPath = Path.Combine(wwwrootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// ═══════════════════════════════════════════════════════════════
// uploads/ — pipeline branch BEFORE UseStaticFiles
// يمنع UseStaticFiles من التعامل مع uploads/* مباشرة.
// يتحقق من الامتداد والوجود ويُعيد الملف مع Content-Type صحيح.
// ✅ يطابق URL القديم: {origin}/uploads/{path}
// ═══════════════════════════════════════════════════════════════
{
    var uploadExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt"
    };

    app.Map("/uploads", (IApplicationBuilder uploadsBranch) =>
    {
        uploadsBranch.Use(async (HttpContext context, RequestDelegate next) =>
        {
            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var dir = Path.Combine(env.WebRootPath, "uploads");
            var reqPath = context.Request.Path.Value?.TrimStart('/') ?? "";
            var sanitized = reqPath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(dir, sanitized));

            // منع path traversal
            if (!fullPath.StartsWith(dir) || !System.IO.File.Exists(fullPath))
            {
                context.Response.StatusCode = 404;
                return;
            }

            // فحص الامتداد
            var ext = Path.GetExtension(fullPath);
            if (!uploadExtensions.Contains(ext))
            {
                context.Response.StatusCode = 403;
                return;
            }

            // Content-Type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            context.Response.Headers["Cache-Control"] = "private, max-age=86400";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            await context.Response.SendFileAsync(fullPath);
        });
    });
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootPath),
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

app.UseMiddleware<JwtCookieMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseMiddleware<AuditLogMiddleware>();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// SPA fallback معطل — UI يُخدم من موقع IIS منفصل (PensionUI) عبر URL Rewrite

app.Run();
