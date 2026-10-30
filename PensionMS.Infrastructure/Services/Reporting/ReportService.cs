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

}
