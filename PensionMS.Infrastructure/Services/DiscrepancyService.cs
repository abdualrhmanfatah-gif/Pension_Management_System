using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Color = System.Drawing.Color;
using System.Drawing;
using System.Diagnostics;

namespace PensionMS.Infrastructure.Services;

public class DiscrepancyService : IDiscrepancyService
{
    private readonly IUnitOfWork _unitOfWork;
    private static bool _fontsRegistered;

    public DiscrepancyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static void RegisterFonts()
    {
        if (_fontsRegistered) return;
        try
        {
            QuestPDF.Settings.UseEnvironmentFonts = false;
            var fontDir = Path.Combine(AppContext.BaseDirectory, "fonts");
            if (Directory.Exists(fontDir))
            {
                foreach (var file in Directory.GetFiles(fontDir, "*.ttf"))
                {
                    using var stream = File.OpenRead(file);
                    FontManager.RegisterFont(stream);
                }
            }
            _fontsRegistered = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiscrepancyService] Font error: {ex.Message}");
            _fontsRegistered = true;
        }
    }

    public async Task<ApiResponse<PagedResult<RetireeDiscrepancyDto>>> GetDiscrepancyReportAsync(DiscrepancyFilterDto filter)
    {
        var query = BuildBaseQuery();

        // Apply Filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(x =>
                x.ApprovalFullName!.Contains(filter.SearchTerm) ||
                x.RetireeFullName!.Contains(filter.SearchTerm) ||
                x.MilitaryNumber!.Contains(filter.SearchTerm) ||
                x.FileNumber!.Contains(filter.SearchTerm)
            );
        }

        if (filter.OnlyMismatches == true)
        {
            query = query.Where(x =>
                (x.ApprovalFullName != x.RetireeFullName && x.RetireeFullName != null) ||
                (x.ApprovalRankName != x.FileRankName && x.FileRankName != null) ||
                (x.ApprovalRetirementDate != x.FileRetirementDate && x.FileRetirementDate != null)
            );
        }

        if (!string.IsNullOrEmpty(filter.Stage))
        {
            if (filter.Stage == "Stage 1") query = query.Where(x => x.RetireeID == null);
            else if (filter.Stage == "Stage 2") query = query.Where(x => x.RetireeID != null && x.FileID == null);
            else if (filter.Stage == "Stage 3") query = query.Where(x => x.FileID != null);
        }

        // Apply Kashf Report Filters
        query = ApplyExtendedFilters(query, filter);

        // Count & Paging
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.ApprovalItemID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<RetireeDiscrepancyDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<RetireeDiscrepancyDto>>.Success(result);
    }

    public async Task<ApiResponse<KashfReportSummaryDto>> GetKashfReportSummaryAsync(DiscrepancyFilterDto filter)
    {
        var query = BuildBaseQuery();
        query = ApplyExtendedFilters(query, filter);

        var allItems = await query.ToListAsync();

        var summary = new KashfReportSummaryDto
        {
            TotalCount = allItems.Count,
            ProcessedCount = allItems.Count(x => x.IsProcessed),
            PendingCount = allItems.Count(x => !x.IsProcessed),
            FileCreatedCount = allItems.Count(x => x.FileID.HasValue),
            ByUnit = allItems
                .Where(x => !string.IsNullOrEmpty(x.ApprovalUnitName))
                .GroupBy(x => x.ApprovalUnitName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByRank = allItems
                .Where(x => !string.IsNullOrEmpty(x.ApprovalRankName))
                .GroupBy(x => x.ApprovalRankName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByCategory = allItems
                .Where(x => !string.IsNullOrEmpty(x.CategoryName))
                .GroupBy(x => x.CategoryName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByGender = allItems
                .GroupBy(x => x.Gender)
                .Select(g => new NameValuePairDto
                {
                    Name = g.Key == 2 ? "أنثى" : "ذكر",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList()
        };

        return ApiResponse<KashfReportSummaryDto>.Success(summary);
    }

    public async Task<byte[]> ExportKashfReportAsync(DiscrepancyFilterDto filter, string format)
    {
        var query = BuildBaseQuery();
        query = ApplyExtendedFilters(query, filter);

        var items = await query.OrderByDescending(x => x.ApprovalItemID).ToListAsync();

        if (format.ToLower() == ExportFormatConstants.Excel)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("تقرير كشوفات الإحالة");
            ws.View.RightToLeft = true;

            var headers = new[] { "م", "الرقم العسكري", "الرتبة", "الاسم الكامل", "الوحدة", "الفئة", "الربط", "الجنس", "المعاش", "تاريخ الإحالة", "الحالة", "الملف" };
            for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                ws.Cells[i + 2, 1].Value = i + 1;
                ws.Cells[i + 2, 2].Value = it.MilitaryNumber;
                ws.Cells[i + 2, 3].Value = it.ApprovalRankName;
                ws.Cells[i + 2, 4].Value = it.ApprovalFullName;
                ws.Cells[i + 2, 5].Value = it.ApprovalUnitName;
                ws.Cells[i + 2, 6].Value = it.CategoryName;
                ws.Cells[i + 2, 7].Value = it.LinkNumber;
                ws.Cells[i + 2, 8].Value = it.Gender == 2 ? "أنثى" : "ذكر";
                ws.Cells[i + 2, 9].Value = it.Salary;
                ws.Cells[i + 2, 10].Value = it.ApprovalRetirementDate?.ToString("yyyy/MM/dd");
                ws.Cells[i + 2, 11].Value = it.IsProcessed ? "معالج" : "معلق";
                ws.Cells[i + 2, 12].Value = it.FileID.HasValue ? "ملف مكتمل" : (it.IsProcessed ? "تمت المعالجة" : "كشف إحالة فقط");
            }

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
        else if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            RegisterFonts();
            QuestPDF.Settings.DocumentLayoutExceptionThreshold = 1000;
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.ContentFromRightToLeft();
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Noto Kufi Arabic"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("الجمهورية اليمنية").Bold();
                            col.Item().Text("وزارة الداخلية");
                            col.Item().Text("صندوق التقاعد الأمني");
                        });
                        row.ConstantItem(120).AlignCenter().Text("تقرير كشوفات الإحالة إلى التقاعد").FontSize(14).Bold();
                        row.RelativeItem().AlignLeft().Column(col =>
                        {
                            col.Item().Text($"التاريخ: {PensionMS.Core.Time.YemenTime.Now:yyyy/MM/dd}");
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(25);
                            cols.ConstantColumn(60);
                            cols.ConstantColumn(60);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(50);
                            cols.ConstantColumn(50);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("م");
                            h.Cell().Element(CellStyle).Text("الرقم العسكري");
                            h.Cell().Element(CellStyle).Text("الرتبة");
                            h.Cell().Element(CellStyle).Text("الاسم الكامل");
                            h.Cell().Element(CellStyle).Text("الوحدة");
                            h.Cell().Element(CellStyle).Text("الجنس");
                            h.Cell().Element(CellStyle).Text("تاريخ الإحالة");
                            h.Cell().Element(CellStyle).Text("الحالة");

                            static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).AlignCenter();
                        });

                        int rowNumber = 1;
                        foreach (var it in items)
                        {
                            table.Cell().Element(ContentStyle).Text(rowNumber.ToString());
                            table.Cell().Element(ContentStyle).Text(it.MilitaryNumber ?? "");
                            table.Cell().Element(ContentStyle).Text(it.ApprovalRankName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.ApprovalFullName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.ApprovalUnitName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.Gender == 2 ? "أنثى" : "ذكر");
                            table.Cell().Element(ContentStyle).Text(it.ApprovalRetirementDate?.ToString("yyyy/MM/dd") ?? "---");
                            table.Cell().Element(ContentStyle).Text(it.IsProcessed ? "معالج" : "معلق");

                            static IContainer ContentStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter();
                            rowNumber++;
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("صفحة ");
                        x.CurrentPageNumber();
                        x.Span(" من ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        return Array.Empty<byte>();
    }

    private IQueryable<RetireeDiscrepancyDto> BuildBaseQuery()
    {
        return from ai in _unitOfWork.MinistryApprovalItems.Query()
                    .AsNoTracking()
                    .Include(i => i.Rank)
                    .Include(i => i.Category)
                join r in _unitOfWork.Retirees.Query() on ai.ApprovalItemID equals r.ApprovalItemID into rJoin
                from r in rJoin.DefaultIfEmpty()
                join rf in _unitOfWork.RetirementFiles.Query() on r.RetireeID equals rf.RetireeID into rfJoin
                from rf in rfJoin.DefaultIfEmpty()
                select new RetireeDiscrepancyDto
                {
                    ApprovalItemID = ai.ApprovalItemID,
                    RetireeID = r != null ? r.RetireeID : (int?)null,
                    FileID = rf != null ? rf.FileID : (int?)null,

                    MilitaryNumber = ai.MilitaryNumber,
                    NationalID = r != null ? r.NationalID : null,

                    ApprovalFullName = ai.FullName,
                    ApprovalRankName = ai.Rank != null ? ai.Rank.RankName : null,
                    ApprovalRetirementDate = ai.RetirementDate,
                    ApprovalUnitName = ai.UnitName,
                    Gender = ai.Gender,
                    Salary = ai.Salary,
                    CategoryName = ai.Category != null ? ai.Category.CategoryName : null,
                    LinkNumber = ai.LinkNumber,

                    RetireeFullName = r != null ? r.FullName : null,
                    IsProcessed = ai.IsProcessed,

                    FileNumber = rf != null ? rf.FileNumber : null,
                    FileRankName = rf != null && rf.Rank != null ? rf.Rank.RankName : null,
                    FileRetirementDate = rf != null ? rf.RetirementDate : null,
                    FileStatusName = rf != null && rf.Status != null ? rf.Status.StatusName : null
                };
    }

    private static IQueryable<RetireeDiscrepancyDto> ApplyExtendedFilters(IQueryable<RetireeDiscrepancyDto> query, DiscrepancyFilterDto filter)
    {
        if (!string.IsNullOrEmpty(filter.UnitName))
            query = query.Where(x => x.ApprovalUnitName == filter.UnitName);

        if (!string.IsNullOrEmpty(filter.RankName))
            query = query.Where(x => x.ApprovalRankName == filter.RankName);

        if (!string.IsNullOrEmpty(filter.CategoryName))
            query = query.Where(x => x.CategoryName == filter.CategoryName);

        if (filter.Gender.HasValue)
            query = query.Where(x => x.Gender == filter.Gender.Value);

        if (!string.IsNullOrEmpty(filter.ProcessingStatus))
        {
            if (filter.ProcessingStatus == "processed")
                query = query.Where(x => x.IsProcessed);
            else if (filter.ProcessingStatus == "pending")
                query = query.Where(x => !x.IsProcessed);
        }

        if (!string.IsNullOrEmpty(filter.FileStatus))
        {
            if (filter.FileStatus == "file_created")
                query = query.Where(x => x.FileID.HasValue);
            else if (filter.FileStatus == "processed_only")
                query = query.Where(x => x.IsProcessed && !x.FileID.HasValue);
            else if (filter.FileStatus == "referral_only")
                query = query.Where(x => !x.IsProcessed);
        }

        return query;
    }
}
