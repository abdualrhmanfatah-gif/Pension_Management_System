using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Core.Interfaces;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services;

public partial class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMissingDocumentService _missingDocumentService;

    public ReportService(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService)
    {
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

    public async Task<byte[]> ExportRetireesAsync(string? search = null, string format = "excel")
    {
        var query = _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Include(r => r.SocialStatus)
            .Include(r => r.HealthStatus)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.FullName.Contains(search) || r.NationalID.Contains(search));

        var data = await query.OrderBy(r => r.FullName).ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المتقاعدون");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "الاسم الكامل", "الهوية الوطنية", "المحافظة", "الهاتف", "الحالة الاجتماعية", "الحالة الصحية", "تاريخ التسجيل" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = r.FullName;
            ws.Cells[row, 3].Value = r.NationalID;
            ws.Cells[row, 4].Value = r.Governorate;
            ws.Cells[row, 5].Value = r.Phone;
            ws.Cells[row, 6].Value = r.SocialStatus?.StatusName ?? "";
            ws.Cells[row, 7].Value = r.HealthStatus?.HealthStatusName ?? "";
            ws.Cells[row, 8].Value = r.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المتقاعدين: {data.Count}");

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportRetirementFilesAsync(int? statusId = null, string format = "excel")
    {
        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Status)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Where(f => !f.IsDeleted);

        if (statusId.HasValue)
            query = query.Where(f => f.StatusID == statusId.Value);

        var data = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("ملفات التقاعد");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "رقم الملف", "اسم المتقاعد", "الرتبة", "الفرع", "الحالة", "تاريخ التقاعد", "تاريخ الإنشاء" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var f = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = f.FileNumber;
            ws.Cells[row, 3].Value = f.Retiree?.FullName ?? "";
            ws.Cells[row, 4].Value = f.Rank?.RankName ?? "";
            ws.Cells[row, 5].Value = f.PensionBranch?.BranchName ?? "";
            ws.Cells[row, 6].Value = f.Status?.StatusName ?? "";
            ws.Cells[row, 7].Value = f.FileOpenDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            ws.Cells[row, 8].Value = f.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الملفات: {data.Count}");

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportEntitledPersonsAsync(int? fileId = null, string format = "excel")
    {
        var query = _unitOfWork.EntitledPersons.Query()
            .AsNoTracking()
            .Include(e => e.RetirementFile)
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Where(e => !e.IsDeleted);

        if (fileId.HasValue)
            query = query.Where(e => e.FileID == fileId.Value);

        var data = await query.OrderBy(e => e.FullName).ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المستحقون");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "الاسم الكامل", "الهوية الوطنية", "صلة القرابة", "رقم الملف", "الحالة الاجتماعية", "الحالة الصحية", "الهاتف" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var e = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = e.FullName;
            ws.Cells[row, 3].Value = e.NationalID;
            ws.Cells[row, 4].Value = e.Relationship?.RelationshipName ?? "";
            ws.Cells[row, 5].Value = e.RetirementFile?.FileNumber ?? "";
            ws.Cells[row, 6].Value = e.SocialStatus?.StatusName ?? "";
            ws.Cells[row, 7].Value = e.HealthStatus?.HealthStatusName ?? "";
            ws.Cells[row, 8].Value = e.Phone;
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المستحقين: {data.Count}");

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportUsersAsync(string format = "excel")
    {
        var data = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المستخدمون");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "الاسم الكامل", "اسم المستخدم", "البريد الإلكتروني", "الدور", "الفرع", "الحالة", "آخر دخول" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var u = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = u.FullName;
            ws.Cells[row, 3].Value = u.Username;
            ws.Cells[row, 4].Value = u.Email;
            ws.Cells[row, 5].Value = u.Role?.RoleName ?? "";
            ws.Cells[row, 6].Value = u.Branch?.BranchName ?? "";
            ws.Cells[row, 7].Value = u.IsActive ? "فعال" : "معطل";
            ws.Cells[row, 8].Value = u.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "";
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المستخدمين: {data.Count}");

        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════

    private static void StyleHeader(ExcelWorksheet ws, int row, int colCount)
    {
        var range = ws.Cells[row, 1, row, colCount];
        range.Style.Font.Bold = true;
        range.Style.Font.Size = 12;
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

        ws.Row(row).Height = 28;
    }

    private static void AutoFitColumns(ExcelWorksheet ws, int colCount)
    {
        for (int i = 1; i <= colCount; i++)
        {
            ws.Column(i).AutoFit();
            if (ws.Column(i).Width < 12)
                ws.Column(i).Width = 12;
        }
    }

    private static void AddFooter(ExcelWorksheet ws, int row, int colCount, string text)
    {
        ws.Cells[row, 1, row, colCount].Merge = true;
        ws.Cells[row, 1].Value = text;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 11;
        ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        ws.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    }
}