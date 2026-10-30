using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public class RetirementFileReportExporter
{
    private readonly ExcelReportExporter _excelExporter;
    private readonly PdfReportExporter _pdfExporter;

    public RetirementFileReportExporter(ExcelReportExporter excelExporter, PdfReportExporter pdfExporter)
    {
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    public byte[] Export(IReadOnlyList<RetirementFileReportRow> items, string format)
    {
        var headers = new[] { "م", "رقم الملف", "المتقاعد", "الرقم العسكري", "الرتبة", "الفرع", "الجنس", "الحالة الاجتماعية", "تاريخ التقاعد", "حالة الملفات", "مراجع", "مستندات ناقصة" };
        var rows = items.Select((it, i) => new object?[]
        {
            i + 1,
            it.FileNumber,
            it.RetireeName,
            it.MilitaryNumber,
            it.RankName,
            it.BranchName,
            it.Gender == 2 ? "أنثى" : "ذكر",
            it.SocialStatusName,
            it.RetirementDate?.ToString("yyyy/MM/dd"),
            it.StatusName,
            it.ReviewStatusName,
            it.MissingDocumentsCount,
        }).ToArray();

        if (format.ToLower() == ExportFormatConstants.Excel)
            return _excelExporter.ExportRows("تقرير ملفات التقاعد", headers, rows,
                new ExcelReportOptions { IncludeFooter = false });

        if (format.ToLower() == ExportFormatConstants.Pdf)
            return _pdfExporter.ExportRows("تقرير ملفات التقاعد", headers, rows);

        return Array.Empty<byte>();
    }
}
