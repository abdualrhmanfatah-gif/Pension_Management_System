using System.Globalization;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Time;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public class UploaderStatsReportExporter
{
    private readonly ExcelReportExporter _excelExporter;

    public UploaderStatsReportExporter(ExcelReportExporter excelExporter) => _excelExporter = excelExporter;

    public byte[] ExportUploads(IReadOnlyList<UploaderStatsDto> data, UploaderStatsFilterDto filter) =>
        _excelExporter.ExportRows("إحصائيات مدخلي الوثائق",
            new[] { "#", "المستخدم", "عدد الوثائق", "عدد الملفات", "آخر رفع" },
            data.Select((row, index) => new object?[] { index + 1, Name(row.FullName, row.UserName),
                row.TotalUploads, row.UniqueFiles, Timestamp(row.LastUploadDate) }).ToArray(),
            new ExcelReportOptions { Details = Period(filter), FooterLabelSpan = 2, FooterText = "الإجمالي",
                NumberFormats = new Dictionary<int, string> { [3] = "#,##0", [4] = "#,##0" },
                FooterValues = new Dictionary<int, decimal> { [3] = data.Sum(row => row.TotalUploads), [4] = data.Sum(row => row.UniqueFiles) } });

    public byte[] ExportEntries(IReadOnlyList<DataEntryStatsDto> data, UploaderStatsFilterDto filter) =>
        _excelExporter.ExportRows("إحصائيات مدخلي البيانات",
            new[] { "#", "المستخدم", "إجمالي الملفات", "مكتملة", "قيد المعالجة", "مرفوضة", "آخر إدخال" },
            data.Select((row, index) => new object?[] { index + 1, Name(row.FullName, row.UserName), row.TotalFilesCreated,
                row.CompletedFiles, row.PendingFiles, row.RejectedFiles, Timestamp(row.LastCreatedDate) }).ToArray(),
            new ExcelReportOptions { Details = Period(filter), FooterLabelSpan = 2, FooterText = "الإجمالي",
                NumberFormats = new Dictionary<int, string> { [3] = "#,##0", [4] = "#,##0", [5] = "#,##0", [6] = "#,##0" },
                FooterValues = new Dictionary<int, decimal> { [3] = data.Sum(row => row.TotalFilesCreated),
                    [4] = data.Sum(row => row.CompletedFiles), [5] = data.Sum(row => row.PendingFiles), [6] = data.Sum(row => row.RejectedFiles) } });

    private static string Name(string? fullName, string userName) => string.IsNullOrWhiteSpace(fullName) ? userName : fullName;

    private static string Timestamp(DateTime? value) => value.HasValue
        ? YemenTime.LocalTime(new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)))
            .ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) : "—";

    private static string Period(UploaderStatsFilterDto filter)
    {
        if (filter.Period is not ("today" or "yesterday" or "week" or "month" or "year")
            && (filter.FromDate.HasValue || filter.ToDate.HasValue))
            return $"الفترة: {filter.FromDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) ?? "البداية"} — {filter.ToDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) ?? "الآن"}";
        return filter.Period switch { "today" => "الفترة: اليوم", "yesterday" => "الفترة: أمس", "week" => "الفترة: الأسبوع",
            "month" => "الفترة: الشهر", "year" => "الفترة: السنة", _ => "الفترة: الكل" };
    }
}
