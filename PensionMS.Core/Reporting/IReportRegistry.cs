namespace PensionMS.Core.Reporting;

public interface IReportRegistry
{
    IReadOnlyList<ReportDefinition> GetAll();
    ReportDefinition? GetById(string id);
    IReadOnlyList<ReportDefinition> GetByCategory(string category);
}
