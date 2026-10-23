using PensionMS.Core.DTOs.Reports;
using ReportFilterDto = PensionMS.Core.DTOs.Reports.ReportFilter;

namespace PensionMS.Core.Interfaces.Reporting;

public interface IReportDataService
{
    Task<object[]> GetDataAsync(string reportId, ReportFilterDto filter);
}
