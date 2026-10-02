using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using PensionMS.Core.Reporting;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

namespace PensionMS.Infrastructure.Reporting;

public class PdfReportExporter
{
    private static readonly string ImagePath = AppContext.BaseDirectory;
    private static bool _fontsRegistered;

    private static void RegisterFonts()
    {
        if (_fontsRegistered) return;

        try
        {
            QuestPDF.Settings.UseEnvironmentFonts = false;

            var fontDir = Path.Combine(AppContext.BaseDirectory, "fonts");

            foreach (var file in Directory.GetFiles(fontDir, "*.ttf"))
            {
                using var stream = File.OpenRead(file);
                FontManager.RegisterFont(stream);
            }

            _fontsRegistered = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PdfReportExporter] Font error: {ex.Message}");
            _fontsRegistered = true;
        }
    }

    public byte[] Export(ReportDefinition report, object[] data)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        RegisterFonts();
        QuestPDF.Settings.DocumentLayoutExceptionThreshold = 5000;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(3, Unit.Millimetre);
                page.MarginVertical(3, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Noto Kufi Arabic"));

                // ═══════════════════════════════════════════════════
                // 1. إطار الصفحة + المحتوى + الشعار فوق المحتوى
                // ═══════════════════════════════════════════════════
                var watermarkPath = Path.Combine(ImagePath, "pension-logo.png");
                byte[]? fadedLogo = null;
                if (File.Exists(watermarkPath))
                {
                    fadedLogo = ApplyTransparency(watermarkPath, 0.12f);
                }

                page.Content().Layers(layers =>
                {
                    // إطار خارجي حول كل صفحة
                    layers.Layer().Padding(2, Unit.Millimetre)
                        .Border(1.2f).BorderColor(Colors.Grey.Darken2);

                    // المحتوى الرئيسي
                    layers.PrimaryLayer()
                        .PaddingHorizontal(5, Unit.Millimetre)
                        .PaddingTop(2, Unit.Millimetre)
                        .PaddingBottom(6, Unit.Millimetre)
                        .Column(contentCol =>
                        {
                            // الترويسة — تظهر مرة واحدة فقط
                            contentCol.Item().ShowOnce().Element(header => ComposePageHeader(header, report, data));

                            // عنوان التقرير
                            contentCol.Item().PaddingVertical(4).AlignCenter().Text(report.NameAr).FontSize(14).Bold();

                            // الجدول
                            contentCol.Item().Element(c => ComposeTable(c, report, data));
                        });

                    // الشعار فوق المحتوى — كل صفحة
                    if (fadedLogo != null)
                    {
                        using var ms = new MemoryStream(fadedLogo);
                        layers.Layer().AlignCenter().AlignMiddle().Width(80, Unit.Millimetre).Image(ms);
                    }
                });

                // ═══════════════════════════════════════════════════
                // 3. الفوتر — تم دمج التذييل المخصص مع ترقيم الصفحات
                // ═══════════════════════════════════════════════════
                page.Footer().Column(col =>
                {
                    ComposeFooter(col);

                    col.Item().AlignCenter().PaddingTop(3).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(7));
                        text.Span("صفحة ").FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().Bold();
                        text.Span(" من ").FontColor(Colors.Grey.Darken1);
                        text.TotalPages().Bold();
                    });
                });
            });
        }).GeneratePdf();
    }

    private void ComposePageHeader(IContainer container, ReportDefinition report, object[] data)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(8, Unit.Millimetre).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                });

                // ═══ اليمين — الشعارات والعنوان ═══
                table.Cell().PaddingLeft(4).Column(rightCol =>
                {
                    rightCol.Item().AlignCenter().Element(c => LoadImageOrDefault(c, "republic_header.png", "الجمهورية اليمنية", 90, 9));
                    rightCol.Item().AlignCenter().PaddingTop(2).Element(c => LoadImageOrDefault(c, "ministry_header.png", "وزارة الداخلية", 75, 8));
                    
                    rightCol.Item().AlignCenter().PaddingTop(1)
                        .Text("الإدارة العامة للصندوق التقاعد الأمني")
                        .FontSize(8).Bold().FontColor(Colors.Blue.Medium);
                });

                // ═══ الوسط — البسملة والشعار ═══
                table.Cell().AlignCenter().AlignMiddle().Column(centerCol =>
                {
                    centerCol.Item().AlignCenter().Element(c => LoadImageOrDefault(c, "basmalah.png", "بسم الله الرحمن الرحيم", 100, 10));
                    centerCol.Item().AlignCenter().PaddingTop(3).Element(c =>
                    {
                        var img = Path.Combine(ImagePath, "pension-logo.png");
                        if (File.Exists(img))
                            c.Width(45).Image(img);
                    });
                });

                // ═══ اليسار — التاريخ والرقم ═══
                table.Cell().AlignLeft().PaddingLeft(8).Column(leftCol =>
                {
                    var now = DateTime.UtcNow;

                    leftCol.Item().AlignRight().Text(text =>
                    {
                        text.Span("التاريـــخ :").Bold().FontSize(8);
                        text.Span("  ");
                        text.Span($"{now:yyyy/MM/dd}").FontSize(8);
                    });

                    leftCol.Item().AlignRight().PaddingTop(2).Text(text =>
                    {
                        text.Span("الرقـــــــم :").Bold().FontSize(8);
                        text.Span("  ");
                        text.Span(report.Id.ToString()).FontSize(8);
                    });

                    leftCol.Item().AlignRight().PaddingTop(2).Text(text =>
                    {
                        text.Span("عدد السجلات:").Bold().FontSize(8);
                        text.Span("  ");
                        text.Span(data.Length.ToString()).FontSize(8);
                    });
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
        });
    }

    // دالة مساعدة لتقليل تكرار الكود وتحميل الصور بأمان
    private void LoadImageOrDefault(IContainer container, string fileName, string fallbackText, int width, int fontSize)
    {
        var imgPath = Path.Combine(ImagePath, fileName);
        if (File.Exists(imgPath))
        {
            try
            {
                container.Width(width).Image(imgPath);
                return;
            }
            catch
            {
                // Fallthrough to text if image is corrupted
            }
        }
        
        container.Text(fallbackText)
            .FontSize(fontSize)
            .Bold()
            .FontColor(Colors.Grey.Darken3);
    }

    private void ComposeTable(IContainer container, ReportDefinition report, object[] data)
    {
        if (data.Length == 0)
        {
            container.AlignCenter().PaddingVertical(15)
                .Text("لا توجد بيانات لعرضها").FontSize(11).FontColor(Colors.Grey.Medium);
            return;
        }

        var columns = report.Columns.ToList();
        int totalWidth = columns.Sum(c => c.Width ?? 30);
        if (totalWidth == 0) totalWidth = columns.Count * 30;

        // ═══ تحسين الأداء: تخزين خصائص النوع مرة واحدة ═══
        Dictionary<string, PropertyInfo> propertyMap = data[0]?.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, PropertyInfo>();

        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                foreach (var column in columns)
                    cols.RelativeColumn((column.Width ?? 30) / (float)totalWidth);
            });

            // ═══ رأس الجدول ═══
            table.Header(h =>
            {
                foreach (var column in columns)
                {
                    h.Cell()
                        .Background(Colors.Blue.Darken3)
                        .PaddingVertical(2).PaddingHorizontal(2)
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(7))
                        .Text(column.Label);                }
            });

            // ═══ صفوف البيانات ═══
            int rowIndex = 0;
            foreach (var item in data)
            {
                bool isEven = rowIndex % 2 == 0;

                foreach (var column in columns)
                {
                    object? value = null;
                    // التحقق من أن المفتاح موجود لتجنب الأخطاء
                    if (!string.IsNullOrEmpty(column.Key) && propertyMap.TryGetValue(column.Key, out var prop))
                    {
                        value = prop.GetValue(item);
                    }

                    var text = FormatCellValue(value, column.Type);
                    if (text.Length > 200) text = text[..197] + "...";

                    table.Cell()
                        .Background(isEven ? Colors.White : Colors.Grey.Lighten4)
                        .BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(2).PaddingHorizontal(2)
                        .DefaultTextStyle(x => x.FontSize(7))
                        .Text(text);
                }
                rowIndex++;
            }
        });
    }

    private void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().BorderTop(0.5f).BorderColor(Colors.Grey.Medium).PaddingTop(3)
            .AlignCenter()
            .Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(6).FontColor(Colors.Grey.Medium));
                text.Span("صادر عن الإدارة العامة للصندوق التقاعد الأمني - وزارة الداخلية");
                text.Span("  |  ");
                text.Span($"تاريخ الاستخراج: {PensionMS.Core.Time.YemenTime.Now:yyyy/MM/dd HH:mm}").FontColor(Colors.Grey.Darken1);
                text.Span("  |  ");
                text.Span("جميع الحقوق محفوظة");
            });
    }

    private static string FormatCellValue(object? value, ReportColumnType columnType)
    {
        // معالجة القيم الفارغة وقيم DBNull القادمة من قواعد البيانات
        if (value == null || value is DBNull) return string.Empty;

        return columnType switch
        {
            ReportColumnType.Currency => value switch
            {
                decimal d => d.ToString("N2"),
                double dbl => dbl.ToString("N2"),
                float f => f.ToString("N2"),
                int i => i.ToString("N2"),
                long l => l.ToString("N2"),
                _ => "0.00"
            },
            ReportColumnType.Number => value switch
            {
                int i => i.ToString("N0"),
                long l => l.ToString("N0"),
                decimal d => d.ToString("N0"),
                double dbl => dbl.ToString("N0"),
                _ => value.ToString() ?? string.Empty
            },
            ReportColumnType.Percent => value switch
            {
                double dbl => $"{dbl:P1}",
                decimal dc => $"{dc:P1}",
                float f => $"{f:P1}",
                _ => value.ToString() ?? string.Empty
            },
            ReportColumnType.Date => value switch
            {
                DateTime dt => dt.ToString("yyyy/MM/dd"),
                DateOnly d => d.ToString("yyyy/MM/dd"),
                _ => value.ToString() ?? string.Empty
            },
            ReportColumnType.Boolean => value is bool b ? (b ? "نعم" : "لا") : "لا",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static byte[]? ApplyTransparency(string imagePath, float opacity)
    {
        try
        {
            using var original = new Bitmap(imagePath);
            var bitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var bytes = new byte[data.Stride * bitmap.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                for (int i = 3; i < bytes.Length; i += 4)
                {
                    bytes[i] = (byte)(bytes[i] * opacity);
                }
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }
}