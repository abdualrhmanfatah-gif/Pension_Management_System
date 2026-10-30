using System.Net;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace PensionMS.ScannerAgent;

public static class WiaConstants
{
    public const int ScannerDeviceType = 1;
    public const string FormatJPEG = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";
    public const uint WIA_ERROR_PAPER_EMPTY = 0x80210003;

    // Property Constants
    public const string DocumentHandlingSelect = "3088";
    public const string Pages = "3096";
    public const string PageSize = "3097";
    public const string HorizontalResolution = "6147";
    public const string VerticalResolution = "6148";
    public const string HorizontalStartPosition = "6149";
    public const string VerticalStartPosition = "6150";
    public const string HorizontalExtent = "6151";
    public const string VerticalExtent = "6152";
}

public class ScannerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

class Program
{
    private static readonly string Url = "http://localhost:5000/";
    private static readonly int MaxAdfPages = 150; // Prevent infinite loops
<<<<<<< HEAD
    private static readonly long MaxTotalScanBytes = 150L * 1024 * 1024; // سقف إجمالي لذاكرة دفعات Base64

    // CORS: تُحمَّل من متغير البيئة SCANNER_ALLOWED_ORIGINS (مفصولة بفواصل) مع قيم افتراضية للتطوير.
    private static readonly string[] AllowedOrigins = LoadAllowedOrigins();

    // جهاز المسح جهاز واحد: طلب واحد في كل مرة (م44) لمنع تداخل عمليات WIA.
    private static readonly System.Threading.SemaphoreSlim ScanLock = new(1, 1);

    private static string[] LoadAllowedOrigins()
    {
        var env = Environment.GetEnvironmentVariable("SCANNER_ALLOWED_ORIGINS");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        return new[]
        {
            "http://localhost:5173", "http://localhost:5141", "http://localhost:3000",
            "http://pension.local", "http://192.168.1.100"
        };
    }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    static void Main(string[] args)
    {
        if (!HttpListener.IsSupported)
        {
            Console.WriteLine("Windows XP SP2 or Server 2003 is required.");
            return;
        }

        using var listener = new HttpListener();
        listener.Prefixes.Add(Url);
        listener.Start();
        Console.WriteLine($"Listening for connections on {Url}");
        Console.WriteLine("Press Ctrl+C to stop.");

        while (true)
        {
            try
            {
                var context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(o => ProcessRequest((HttpListenerContext)o!), context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Listener Error: {ex.Message}");
            }
        }
    }

    static void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

<<<<<<< HEAD
        // م47/م48: قائمة origins من الإعدادات، ورفض صريح لـ Origin غير الموثوق على الخادم.
        var origin = request.Headers["Origin"];
        if (origin != null && !AllowedOrigins.Contains(origin))
        {
            Console.WriteLine($"CORS blocked: origin '{origin}' is not in the allowed origins list.");
            response.StatusCode = 403;
            WriteResponseAndClose(response, "{\"error\":\"origin-not-allowed\"}");
            return;
        }
        if (origin != null)
=======
        // Security fix: Restrict allowed origins to known frontend domains
        var allowedOrigins = new[] { "http://localhost:5173","http://localhost:5141", "http://localhost:3000", "http://pension.local", "http://192.168.1.100" };
        var origin = request.Headers["Origin"];
        if (origin != null && allowedOrigins.Contains(origin))
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        {
            response.Headers.Add("Access-Control-Allow-Origin", origin);
        }
        
        response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        try
        {
            if (request.Url?.AbsolutePath == "/scan" && request.HttpMethod == "POST")
            {
                HandleScan(context);
            }
            else if (request.Url?.AbsolutePath == "/scanners" && request.HttpMethod == "GET")
            {
                HandleListScanners(response);
            }
            else
            {
                response.StatusCode = 404;
                WriteResponseAndClose(response, "Not Found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception in request processing: {ex.Message}");
            response.StatusCode = 500;
            WriteResponseAndClose(response, "Internal Server Error");
        }
    }

    static void HandleListScanners(HttpListenerResponse response)
    {
        object? deviceManager = null;
        object? deviceInfos = null;

        try
        {
            Type? deviceManagerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (deviceManagerType == null) throw new InvalidOperationException("WIA is not installed.");

            deviceManager = Activator.CreateInstance(deviceManagerType);
            if (deviceManager == null) throw new Exception("Failed to instantiate DeviceManager.");

            var scanners = new List<ScannerInfo>();
            deviceInfos = GetProperty(deviceManager, "DeviceInfos");
            int count = (int)GetProperty(deviceInfos, "Count")!;

            for (int i = 1; i <= count; i++)
            {
                object? info = null;
                object? properties = null;
                object? nameProp = null;

                try
                {
                    info = GetIndexer(deviceInfos, i);
                    if (info != null && (int)GetProperty(info, "Type")! == WiaConstants.ScannerDeviceType)
                    {
                        var id = (string)GetProperty(info, "DeviceID")!;
                        properties = GetProperty(info, "Properties");
                        nameProp = GetIndexer(properties, "Name");
                        var name = (string)GetProperty(nameProp, "Value")!;

                        scanners.Add(new ScannerInfo { Id = id, Name = name });
                    }
                }
                finally
                {
                    ReleaseComObject(nameProp);
                    ReleaseComObject(properties);
                    ReleaseComObject(info);
                }
            }

            // Using System.Text.Json instead of StringBuilder JSON building
            string json = JsonSerializer.Serialize(scanners, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            response.ContentType = "application/json";
            WriteResponseAndClose(response, json);
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            // م49: الخطأ (عدم تثبيت WIA مثلًا) يعيد 500 برسالة حقيقية، لا سجلًا فارغًا كأنه نجاح.
            Console.WriteLine($"Error listing scanners: {ex.Message}");
            response.StatusCode = 500;
            response.ContentType = "application/json";
            var errorJson = JsonSerializer.Serialize(new { error = "wia-error", message = ex.Message });
            WriteResponseAndClose(response, errorJson);
=======
            Console.WriteLine($"Error listing scanners: {ex.Message}");
            response.ContentType = "application/json";
            WriteResponseAndClose(response, "[]");
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }
        finally
        {
            // Explicit COM object cleanup for memory leaks
            ReleaseComObject(deviceInfos);
            ReleaseComObject(deviceManager);
        }
    }

    static void HandleScan(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;

        // Validation for DoS / input sanity
        int dpi = 200;
        int widthPixels = 0;
        int heightPixels = 0;
        bool useAdf = false;
<<<<<<< HEAD
        double offsetXInches = 0;
        double offsetYInches = 0;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        if (int.TryParse(request.QueryString["dpi"], out var d) && d is >= 50 and <= 1200) dpi = d;
        if (double.TryParse(request.QueryString["width"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var wInch) && wInch > 0 && wInch <= 17) widthPixels = (int)(wInch * dpi);
        if (double.TryParse(request.QueryString["height"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var hInch) && hInch > 0 && hInch <= 22) heightPixels = (int)(hInch * dpi);
        if (bool.TryParse(request.QueryString["adf"], out var adf)) useAdf = adf;
<<<<<<< HEAD
        // م46: عقد الوحدات إنشات (inches) — تُحوَّل داخل ConfigureItemProperties لوحدات WIA (0.001 inch).
        if (double.TryParse(request.QueryString["offsetX"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ox) && ox >= 0 && ox <= 3) offsetXInches = ox;
        if (double.TryParse(request.QueryString["offsetY"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var oy) && oy >= 0 && oy <= 3) offsetYInches = oy;

        // م44: جهاز المسح جهاز واحد — طلب واحد في كل مرة. رفض فوري (429) بدل تراكم عمليات WIA.
        if (!ScanLock.Wait(0))
        {
            Console.WriteLine("Busy: another scan is already in progress.");
            response.StatusCode = 429;
            WriteResponseAndClose(response, "{\"error\":\"busy\",\"message\":\"scanner busy\"}");
            return;
        }
        var lockHeld = true;

        try
        {
            Console.WriteLine($"Starting scan (DPI: {dpi}, ADF: {useAdf}, offset: {offsetXInches}x{offsetYInches})...");

            // Threading Issue Fix: WIA must run on an STA thread.
            // We use an explicit STA thread with a timeout and state flags.
            Exception? threadException = null;
            byte[]? finalBuffer = null;
            string finalContentType = string.Empty;
            string stopReason = "complete";

            var t = new Thread(() =>
            {
                try
                {
                    PerformWiaScan(dpi, widthPixels, heightPixels, useAdf, offsetXInches, offsetYInches, out finalBuffer, out finalContentType, out stopReason);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            // Timeout prevents scanner hanging resources indefinitely
            if (!t.Join(TimeSpan.FromMinutes(4)))
            {
                // الخيط المعلّق يستمر في التنفيذ؛ نطلق القفل عند اكتماله فعليًا لا هنا (م44).
                Console.WriteLine("Error: WIA Scan timed out. Driver may be hanging.");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    t.Join();
                    ScanLock.Release();
                });
                lockHeld = false;
                response.StatusCode = 504; // Gateway Timeout
                WriteResponseAndClose(response, "{\"error\":\"timeout\",\"message\":\"scan timed out\"}");
                return;
            }

            if (threadException != null)
            {
                string errMsg = threadException.InnerException != null ? threadException.InnerException.Message : threadException.Message;
                Console.WriteLine($"Scan Error: {errMsg}\nStack: {threadException.StackTrace}");
                response.StatusCode = 500;
                WriteResponseAndClose(response, "Scan failed: " + errMsg);
                return;
            }

            if (finalBuffer != null && finalBuffer.Length > 0)
            {
                response.ContentType = finalContentType;
                response.ContentLength64 = finalBuffer.Length;
                response.OutputStream.Write(finalBuffer, 0, finalBuffer.Length);
                Console.WriteLine($"Scan successful, sent {finalBuffer.Length} bytes (stop reason: {stopReason}).");
                response.Close();
            }
            else
            {
                response.StatusCode = 204; // No content
                response.Close();
            }
        }
        finally
        {
            if (lockHeld) ScanLock.Release();
        }
    }

    static void PerformWiaScan(int dpi, int widthPixels, int heightPixels, bool useAdf, double offsetXInches, double offsetYInches, out byte[]? finalBuffer, out string contentType, out string stopReason)
    {
        finalBuffer = null;
        contentType = string.Empty;
        stopReason = "complete";
=======

        Console.WriteLine($"Starting scan (DPI: {dpi}, ADF: {useAdf})...");

        // Threading Issue Fix: WIA must run on an STA thread.
        // We use an explicit STA thread with a timeout and state flags.
        Exception? threadException = null;
        byte[]? finalBuffer = null;
        string finalContentType = string.Empty;

        var t = new Thread(() =>
        {
            try
            {
                PerformWiaScan(dpi, widthPixels, heightPixels, useAdf, out finalBuffer, out finalContentType);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        t.SetApartmentState(ApartmentState.STA);
        t.IsBackground = true; 
        t.Start();
        
        // Timeout prevents scanner hanging resources indefinitely
        if (!t.Join(TimeSpan.FromMinutes(4)))
        {
            Console.WriteLine("Error: WIA Scan timed out. Driver may be hanging.");
            response.StatusCode = 504; // Gateway Timeout
            WriteResponseAndClose(response, "Scan timed out.");
            return;
        }

        if (threadException != null)
        {
            string errMsg = threadException.InnerException != null ? threadException.InnerException.Message : threadException.Message;
            Console.WriteLine($"Scan Error: {errMsg}\nStack: {threadException.StackTrace}");
            response.StatusCode = 500;
            WriteResponseAndClose(response, "Scan failed: " + errMsg);
            return;
        }

        if (finalBuffer != null && finalBuffer.Length > 0)
        {
            response.ContentType = finalContentType;
            response.ContentLength64 = finalBuffer.Length;
            response.OutputStream.Write(finalBuffer, 0, finalBuffer.Length);
            Console.WriteLine($"Scan successful, sent {finalBuffer.Length} bytes.");
            response.Close();
        }
        else
        {
            response.StatusCode = 204; // No content
            response.Close();
        }
    }

    static void PerformWiaScan(int dpi, int widthPixels, int heightPixels, bool useAdf, out byte[]? finalBuffer, out string contentType)
    {
        finalBuffer = null;
        contentType = string.Empty;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var tempFiles = new List<string>();

        object? deviceManager = null;
        object? deviceInfos = null;
        object? firstScannerInfo = null;
        object? device = null;
        object? items = null;
        object? firstItem = null;

        try
        {
            Type? deviceManagerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (deviceManagerType == null) throw new Exception("WIA DeviceManager not found.");

            deviceManager = Activator.CreateInstance(deviceManagerType);
            deviceInfos = GetProperty(deviceManager, "DeviceInfos");
            int count = (int)GetProperty(deviceInfos, "Count")!;

            for (int i = 1; i <= count; i++)
            {
                object? info = GetIndexer(deviceInfos, i);
                if (info != null && (int)GetProperty(info, "Type")! == WiaConstants.ScannerDeviceType)
                {
                    firstScannerInfo = info;
                    break;
                }
                ReleaseComObject(info);
            }

            if (firstScannerInfo == null) throw new Exception("No scanner found.");

            int connectRetries = 3;
            for (int r = 0; r < connectRetries; r++)
            {
                try 
                {
                    device = InvokeMethod(firstScannerInfo, "Connect");
                    break;
                }
                catch (Exception ex)
                {
                    if (r == connectRetries - 1) throw;
                    Console.WriteLine($"Scanner busy ({ex.InnerException?.Message ?? ex.Message}), retrying in 1.5s...");
                    Thread.Sleep(1500);
                }
            }
            
            if (device == null) throw new Exception("Failed to connect to scanner after retries.");

            ConfigureDeviceAdf(device, useAdf);

            items = GetProperty(device, "Items");
<<<<<<< HEAD
            firstItem = GetIndexer(items, 1) ?? throw new Exception("Scanner has no scan items.");

            ConfigureItemProperties(firstItem, dpi, widthPixels, heightPixels, useAdf, offsetXInches, offsetYInches);
=======
            firstItem = GetIndexer(items, 1);

            ConfigureItemProperties(firstItem, dpi, widthPixels, heightPixels, useAdf);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            Console.WriteLine("Transferring image(s)...");
            
            bool hasMorePages = true;
            int pageCount = 0;
<<<<<<< HEAD
            long totalBytes = 0;
            bool bytesExceeded = false;
            string? transferError = null;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            // Explicit maximum iteration count prevents infinite ADF feed loop
            while (hasMorePages && pageCount < MaxAdfPages)
            {
                object? image = null;
                try
                {
                    image = InvokeMethod(firstItem, "Transfer", WiaConstants.FormatJPEG);
                    if (image != null)
                    {
                        string tempPath = Path.Combine(Path.GetTempPath(), "scan_" + Guid.NewGuid() + ".jpg");
                        InvokeMethod(image, "SaveFile", tempPath);
                        tempFiles.Add(tempPath);
                        pageCount++;
                        Console.WriteLine($"Scanned page {pageCount}.");
<<<<<<< HEAD

                        // م43: سقف إجمالي يمنع تضخّم ذاكرة Base64 مع آلاف الصفحات.
                        totalBytes += new FileInfo(tempPath).Length;
                        if (totalBytes > MaxTotalScanBytes)
                        {
                            bytesExceeded = true;
                            Console.WriteLine($"Total scan size exceeded {MaxTotalScanBytes} bytes. Stopping.");
                            break;
                        }

=======
                        
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        if (!useAdf) break;
                    }
                    else
                    {
                        hasMorePages = false;
                    }
                }
                catch (Exception ex)
                {
                    var comEx = ex as COMException ?? ex.InnerException as COMException;
<<<<<<< HEAD

=======
                    
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    if (comEx != null && ((uint)comEx.ErrorCode == WiaConstants.WIA_ERROR_PAPER_EMPTY || comEx.Message.Contains("no documents left")))
                    {
                        Console.WriteLine("ADF is empty. Finished scanning.");
                    }
                    else if (comEx != null)
                    {
                        Console.WriteLine($"COM Exception during transfer: {comEx.Message} (Code: {comEx.ErrorCode:X})");
                        if (pageCount == 0) throw;
<<<<<<< HEAD
                        transferError = comEx.Message;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    }
                    else
                    {
                        Console.WriteLine($"Transfer Exception: {ex.InnerException?.Message ?? ex.Message}");
                        if (pageCount == 0) throw;
<<<<<<< HEAD
                        transferError = ex.InnerException?.Message ?? ex.Message;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    }
                    hasMorePages = false;
                }
                finally
                {
                    ReleaseComObject(image);
                }
            }

<<<<<<< HEAD
            // م43: تقرير صريح لسبب توقف المسح — دفعة جزئية لا تُقدَّم كاكتمال.
            if (bytesExceeded) stopReason = "max-bytes";
            else if (pageCount >= MaxAdfPages && hasMorePages) stopReason = "max-pages";
            else if (transferError != null) stopReason = "error: " + transferError;
            else stopReason = "complete";

            if (tempFiles.Count > 0)
            {
                // م46: إزاحة القص البديل بوحدات بكسل — تُطبَّق فقط حين يمسح الجهاز سريرًا كاملًا أكبر من المطلوب.
                int offsetXPixels = (int)Math.Round(offsetXInches * dpi);
                int offsetYPixels = (int)Math.Round(offsetYInches * dpi);

                if (useAdf || tempFiles.Count > 1)
                {
                    finalBuffer = CreateJsonImages(tempFiles, widthPixels, heightPixels, offsetXPixels, offsetYPixels, stopReason);
=======
            if (tempFiles.Count > 0)
            {
                if (useAdf || tempFiles.Count > 1)
                {
                    finalBuffer = CreateJsonImages(tempFiles, widthPixels, heightPixels);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    contentType = "application/json";
                }
                else
                {
<<<<<<< HEAD
                    finalBuffer = ProcessImage(tempFiles[0], widthPixels, heightPixels, offsetXPixels, offsetYPixels, out _);
=======
                    finalBuffer = ProcessImage(tempFiles[0], widthPixels, heightPixels, out _);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    contentType = "image/jpeg";
                }
            }
        }
        finally
        {
            // Explicit COM cleanup - critical for preventing scanner locks
            ReleaseComObject(firstItem);
            ReleaseComObject(items);
            ReleaseComObject(device);
            ReleaseComObject(firstScannerInfo);
            ReleaseComObject(deviceInfos);
            ReleaseComObject(deviceManager);

            // Cleanup secure temp files from the drive in all scenarios
            foreach (var file in tempFiles)
            {
                try { if (File.Exists(file)) File.Delete(file); } catch { }
            }
        }
    }

    static void ConfigureDeviceAdf(object device, bool useAdf)
    {
        object? properties = null;
        try
        {
            properties = GetProperty(device, "Properties");
            if (useAdf)
            {
                TrySetWiaPropertySafe(properties, WiaConstants.DocumentHandlingSelect, 1); // 1 = Feeder
                TrySetWiaPropertySafe(properties, WiaConstants.Pages, 1); // 1 page per transfer
            }
            else
            {
                TrySetWiaPropertySafe(properties, WiaConstants.DocumentHandlingSelect, 2); // 2 = Flatbed
            }
        }
        finally
        {
            ReleaseComObject(properties);
        }
    }

<<<<<<< HEAD
    static void ConfigureItemProperties(object item, int dpi, int width, int height, bool useAdf, double offsetXInches, double offsetYInches)
=======
    static void ConfigureItemProperties(object item, int dpi, int width, int height, bool useAdf)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        object? properties = null;
        try
        {
            properties = GetProperty(item, "Properties");
<<<<<<< HEAD

            TrySetWiaPropertySafe(properties, WiaConstants.PageSize, 0); // Custom format
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalResolution, dpi);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalResolution, dpi);
            // م46: مواضع البداية بوحدات WIA = 0.001 inch؛ تدخل كسنتي إنش عادة (0.1 للجواز/الصورة الشخصية).
            int startX = (int)Math.Round(offsetXInches * 1000);
            int startY = (int)Math.Round(offsetYInches * 1000);
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalStartPosition, startX);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalStartPosition, startY);
=======
            
            TrySetWiaPropertySafe(properties, WiaConstants.PageSize, 0); // Custom format
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalResolution, dpi);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalResolution, dpi);
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalStartPosition, 0);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalStartPosition, 0);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            if (width > 0) TrySetWiaPropertySafe(properties, WiaConstants.HorizontalExtent, width);
            if (height > 0) TrySetWiaPropertySafe(properties, WiaConstants.VerticalExtent, height);

            if (useAdf)
            {
                TrySetWiaPropertySafe(properties, WiaConstants.Pages, 1);
            }
        }
        finally
        {
            ReleaseComObject(properties);
        }
    }

<<<<<<< HEAD
    static byte[] CreateJsonImages(List<string> tempFiles, int widthPixels, int heightPixels, int offsetXPixels, int offsetYPixels, string stopReason)
=======
    static byte[] CreateJsonImages(List<string> tempFiles, int widthPixels, int heightPixels)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        Console.WriteLine("Creating JSON response with separate scanned images...");
        var list = new List<object>();
        for (int i = 0; i < tempFiles.Count; i++)
        {
<<<<<<< HEAD
            byte[] pageBytes = ProcessImage(tempFiles[i], widthPixels, heightPixels, offsetXPixels, offsetYPixels, out _);
            list.Add(new { 
                name = $"scan_page_{i + 1}_{DateOnly.FromDateTime(DateTime.UtcNow):yyyyMMdd}.jpg", 
=======
            byte[] pageBytes = ProcessImage(tempFiles[i], widthPixels, heightPixels, out _);
            list.Add(new { 
                name = $"scan_page_{i + 1}_{DateOnly.FromDateTime(DateTime.Now):yyyyMMdd}.jpg", 
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                type = "image/jpeg", 
                data = Convert.ToBase64String(pageBytes) 
            });
        }
<<<<<<< HEAD
        // م43: الحاوية تصرح باكتمال الدفعة وسبب التوقف — لا تُقدَّم دفعة جزئية كأنها نجاح.
        var payload = new
        {
            complete = stopReason == "complete",
            pageCount = tempFiles.Count,
            stopReason,
            pages = list
        };
        string jsonResponse = System.Text.Json.JsonSerializer.Serialize(payload);
        return Encoding.UTF8.GetBytes(jsonResponse);
    }

    static byte[] ProcessImage(string imagePath, int widthPixels, int heightPixels, int offsetXPixels, int offsetYPixels, out bool wasCropped)
=======
        string jsonResponse = System.Text.Json.JsonSerializer.Serialize(list);
        return Encoding.UTF8.GetBytes(jsonResponse);
    }

    static byte[] ProcessImage(string imagePath, int widthPixels, int heightPixels, out bool wasCropped)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        wasCropped = false;
        if (widthPixels <= 0 || heightPixels <= 0) return File.ReadAllBytes(imagePath);

        try 
        {
            using var original = new Bitmap(imagePath);
            
            // If the image is exactly the requested size or slightly smaller, don't crop
<<<<<<< HEAD
            // (الجهاز احترم Extent/Position — الإزاحة طُبِّقت أصلًا عبر WIA).
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (original.Width <= widthPixels + 2 && original.Height <= heightPixels + 2)
            {
                return File.ReadAllBytes(imagePath);
            }

<<<<<<< HEAD
            // م46: القص البديل لجهاز يمسح السرير كاملًا رغم الإعدادات — يبدأ من موضع الإزاحة لا من الزاوية العلوية اليسرى.
            int srcX = Math.Max(0, Math.Min(offsetXPixels, original.Width - 1));
            int srcY = Math.Max(0, Math.Min(offsetYPixels, original.Height - 1));
            int cropW = Math.Min(widthPixels, original.Width - srcX);
            int cropH = Math.Min(heightPixels, original.Height - srcY);
            
            using var cropped = new Bitmap(cropW, cropH);
            using var g = Graphics.FromImage(cropped);
            g.DrawImage(original, new Rectangle(0, 0, cropW, cropH), new Rectangle(srcX, srcY, cropW, cropH), GraphicsUnit.Pixel);
=======
            // We enforce a strict crop to the requested dimensions from top-left.
            // Some drivers scan the whole bed even when given specific Extents.
            int cropW = Math.Min(widthPixels, original.Width);
            int cropH = Math.Min(heightPixels, original.Height);
            
            using var cropped = new Bitmap(cropW, cropH);
            using var g = Graphics.FromImage(cropped);
            g.DrawImage(original, new Rectangle(0, 0, cropW, cropH), new Rectangle(0, 0, cropW, cropH), GraphicsUnit.Pixel);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            
            using var ms = new MemoryStream();
            cropped.Save(ms, ImageFormat.Jpeg);
            wasCropped = true;
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cropping failed: {ex.Message}. Using original.");
            return File.ReadAllBytes(imagePath);
        }
    }

    // --- COM Reflection Helpers (Replaces 'dynamic' for strict safety) ---

    static void TrySetWiaPropertySafe(object? properties, string propertyId, object value)
    {
        if (properties == null) return;
        
        object? prop = null;
        try
        {
            prop = GetIndexer(properties, propertyId);
            if (prop != null)
            {
                bool isReadOnly = (bool)GetProperty(prop, "IsReadOnly")!;
                if (isReadOnly)
                {
                    Console.WriteLine($"WIA property {propertyId} is read-only. Skipping.");
                    return;
                }

                if (value is int intVal)
                {
                    int subType = Convert.ToInt32(GetProperty(prop, "SubType"));
                    if (subType == 1) // WiaSubTypeRange
                    {
                        int min = Convert.ToInt32(GetProperty(prop, "SubTypeMin"));
                        int max = Convert.ToInt32(GetProperty(prop, "SubTypeMax"));
                        int step = Convert.ToInt32(GetProperty(prop, "SubTypeStep"));

                        if (intVal < min) intVal = min;
                        if (intVal > max) intVal = max;

                        if (step > 0 && intVal > min)
                        {
                            int diff = intVal - min;
                            int remainder = diff % step;
                            if (remainder != 0)
                            {
                                intVal = intVal - remainder; // round down to exact valid step
                            }
                        }
                        value = intVal;
                    }
                }

                SetProperty(prop, "Value", value);
                Console.WriteLine($"Set property {propertyId} to {value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set WIA property {propertyId} to {value}: {ex.Message}");
        }
        finally
        {
            ReleaseComObject(prop);
        }
    }

    static object? GetProperty(object? target, string name)
    {
        if (target == null) return null;
        return target.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, target, null);
    }

    static void SetProperty(object? target, string name, object value)
    {
        if (target == null) return;
        target.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, target, new object[] { value });
    }

    static object? InvokeMethod(object? target, string name, params object[] args)
    {
        if (target == null) return null;
        return target.GetType().InvokeMember(name, System.Reflection.BindingFlags.InvokeMethod, null, target, args);
    }

    static object? GetIndexer(object? target, object index)
    {
        if (target == null) return null;
        return target.GetType().InvokeMember("Item", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { index });
    }

    static void ReleaseComObject(object? obj)
    {
        if (obj != null && Marshal.IsComObject(obj))
        {
            Marshal.FinalReleaseComObject(obj);
        }
    }

    static void WriteResponseAndClose(HttpListenerResponse response, string text)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch { }
        finally
        {
            response.Close();
        }
    }
}
