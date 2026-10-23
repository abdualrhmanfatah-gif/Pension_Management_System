using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;

namespace PensionMS.Core.Interfaces.Reporting;

public interface IRetirementFileReportService
{
    Task<ApiResponse<PagedResult<RetirementFileReportRow>>> GetRetirementFileReportAsync(RetirementFileReportFilter filter);
    Task<ApiResponse<RetirementFileReportSummaryDto>> GetRetirementFileReportSummaryAsync(RetirementFileReportFilter filter);
    Task<byte[]> ExportRetirementFileReportAsync(RetirementFileReportFilter filter, string format);
}
