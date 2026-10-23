using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.Interfaces;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly ExcelReportExporter _excelExporter;

    public ReportService(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService, IAccessiblePensionRepository access, ExcelReportExporter excelExporter)
    {
        _unitOfWork = unitOfWork;
        _access = access;
        _missingDocumentService = missingDocumentService;
        _excelExporter = excelExporter;
    }

    /// <summary>
    /// تطبيق التصفحة على IQueryable وإرجاع PagedResult
    /// </summary>
    protected static async Task<PagedResult<T>> ApplyPagingAsync<T>(IQueryable<T> query, ReportFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var totalCount = await query.CountAsync();
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<byte[]> ExportRetireesAsync(string? search = null, string format = "excel", int? branchId = null)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        var query = queries.Query(_unitOfWork.Retirees, branchId)
            .AsNoTracking()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.FullName.Contains(search) || r.NationalID.Contains(search));

        var data = await query.OrderBy(r => r.FullName).ThenBy(r => r.RetireeID).ToListAsync();

        var headers = new[] { "#", "الاسم الكامل", "الهوية الوطنية", "المحافظة", "الهاتف", "الحالة الاجتماعية", "الحالة الصحية", "تاريخ التسجيل" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.FullName,
            r.NationalID,
            r.Governorate,
            r.Phone,
            r.SocialStatus?.StatusName ?? "",
            r.HealthStatus?.HealthStatusName ?? "",
            r.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        }).ToArray();
        return _excelExporter.ExportRows("المتقاعدون", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المتقاعدين: {data.Count}" });
    }

    public async Task<byte[]> ExportRetirementFilesAsync(int? statusId = null, string format = "excel", int? branchId = null)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        var query = queries.Query(_unitOfWork.RetirementFiles, branchId)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Status)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Where(f => !f.IsDeleted);

        if (statusId.HasValue)
            query = query.Where(f => f.StatusID == statusId.Value);

        var data = await query.OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.FileID).ToListAsync();

        var headers = new[] { "#", "رقم الملف", "اسم المتقاعد", "الرتبة", "الفرع", "الحالة", "تاريخ التقاعد", "تاريخ الإنشاء" };
        var rows = data.Select((f, i) => new object?[]
        {
            i + 1,
            f.FileNumber,
            f.Retiree?.FullName ?? "",
            f.Rank?.RankName ?? "",
            f.PensionBranch?.BranchName ?? "",
            f.Status?.StatusName ?? "",
            f.FileOpenDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            f.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        }).ToArray();
        return _excelExporter.ExportRows("ملفات التقاعد", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي الملفات: {data.Count}" });
    }

    public async Task<byte[]> ExportEntitledPersonsAsync(int? fileId = null, string format = "excel", int? branchId = null)
    {
        var queries = await _access.ForPermissionAsync("Report.Entitlements.View");
        var query = queries.Query(_unitOfWork.EntitledPersons, branchId)
            .AsNoTracking()
            .Include(e => e.RetirementFile)
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Where(e => !e.IsDeleted);

        if (fileId.HasValue)
            query = query.Where(e => e.FileID == fileId.Value);

        var data = await query.OrderBy(e => e.FullName).ThenBy(e => e.EntitledID).ToListAsync();

        var headers = new[] { "#", "الاسم الكامل", "الهوية الوطنية", "صلة القرابة", "رقم الملف", "الحالة الاجتماعية", "الحالة الصحية", "الهاتف" };
        var rows = data.Select((e, i) => new object?[]
        {
            i + 1,
            e.FullName,
            e.NationalID,
            e.Relationship?.RelationshipName ?? "",
            e.RetirementFile?.FileNumber ?? "",
            e.SocialStatus?.StatusName ?? "",
            e.HealthStatus?.HealthStatusName ?? "",
            e.Phone,
        }).ToArray();
        return _excelExporter.ExportRows("المستحقون", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المستحقين: {data.Count}" });
    }

    public async Task<byte[]> ExportUsersAsync(string format = "excel", int? branchId = null)
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var data = await queries.Query(_unitOfWork.Users, branchId)
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .OrderBy(u => u.FullName).ThenBy(u => u.UserID)
            .ToListAsync();

        var headers = new[] { "#", "الاسم الكامل", "اسم المستخدم", "البريد الإلكتروني", "الدور", "الفرع", "الحالة", "آخر دخول" };
        var rows = data.Select((u, i) => new object?[]
        {
            i + 1,
            u.FullName,
            u.Username,
            u.Email,
            u.Role?.RoleName ?? "",
            u.Branch?.BranchName ?? "",
            u.IsActive ? "فعال" : "معطل",
            u.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "",
        }).ToArray();
        return _excelExporter.ExportRows("المستخدمون", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المستخدمين: {data.Count}" });
    }

}
