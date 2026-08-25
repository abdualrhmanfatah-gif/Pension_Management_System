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

        // Security fix: Restrict allowed origins to known frontend domains
        var allowedOrigins = new[] { "http://localhost:5173","http://localhost:5141", "http://localhost:3000", "http://pension.local", "http://192.168.1.100" };
        var origin = request.Headers["Origin"];
        if (origin != null && allowedOrigins.Contains(origin))
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
            Console.WriteLine($"Error listing scanners: {ex.Message}");
            response.ContentType = "application/json";
            WriteResponseAndClose(response, "[]");
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

        if (int.TryParse(request.QueryString["dpi"], out var d) && d is >= 50 and <= 1200) dpi = d;
        if (double.TryParse(request.QueryString["width"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var wInch) && wInch > 0 && wInch <= 17) widthPixels = (int)(wInch * dpi);
        if (double.TryParse(request.QueryString["height"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var hInch) && hInch > 0 && hInch <= 22) heightPixels = (int)(hInch * dpi);
        if (bool.TryParse(request.QueryString["adf"], out var adf)) useAdf = adf;

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
            firstItem = GetIndexer(items, 1);

            ConfigureItemProperties(firstItem, dpi, widthPixels, heightPixels, useAdf);

            Console.WriteLine("Transferring image(s)...");
            
            bool hasMorePages = true;
            int pageCount = 0;

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
                    
                    if (comEx != null && ((uint)comEx.ErrorCode == WiaConstants.WIA_ERROR_PAPER_EMPTY || comEx.Message.Contains("no documents left")))
                    {
                        Console.WriteLine("ADF is empty. Finished scanning.");
                    }
                    else if (comEx != null)
                    {
                        Console.WriteLine($"COM Exception during transfer: {comEx.Message} (Code: {comEx.ErrorCode:X})");
                        if (pageCount == 0) throw;
                    }
                    else
                    {
                        Console.WriteLine($"Transfer Exception: {ex.InnerException?.Message ?? ex.Message}");
                        if (pageCount == 0) throw;
                    }
                    hasMorePages = false;
                }
                finally
                {
                    ReleaseComObject(image);
                }
            }

            if (tempFiles.Count > 0)
            {
                if (useAdf || tempFiles.Count > 1)
                {
                    finalBuffer = CreateJsonImages(tempFiles, widthPixels, heightPixels);
                    contentType = "application/json";
                }
                else
                {
                    finalBuffer = ProcessImage(tempFiles[0], widthPixels, heightPixels, out _);
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

    static void ConfigureItemProperties(object item, int dpi, int width, int height, bool useAdf)
    {
        object? properties = null;
        try
        {
            properties = GetProperty(item, "Properties");
            
            TrySetWiaPropertySafe(properties, WiaConstants.PageSize, 0); // Custom format
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalResolution, dpi);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalResolution, dpi);
            TrySetWiaPropertySafe(properties, WiaConstants.HorizontalStartPosition, 0);
            TrySetWiaPropertySafe(properties, WiaConstants.VerticalStartPosition, 0);

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

    static byte[] CreateJsonImages(List<string> tempFiles, int widthPixels, int heightPixels)
    {
        Console.WriteLine("Creating JSON response with separate scanned images...");
        var list = new List<object>();
        for (int i = 0; i < tempFiles.Count; i++)
        {
            byte[] pageBytes = ProcessImage(tempFiles[i], widthPixels, heightPixels, out _);
            list.Add(new { 
                name = $"scan_page_{i + 1}_{DateOnly.FromDateTime(DateTime.Now):yyyyMMdd}.jpg", 
                type = "image/jpeg", 
                data = Convert.ToBase64String(pageBytes) 
            });
        }
        string jsonResponse = System.Text.Json.JsonSerializer.Serialize(list);
        return Encoding.UTF8.GetBytes(jsonResponse);
    }

    static byte[] ProcessImage(string imagePath, int widthPixels, int heightPixels, out bool wasCropped)
    {
        wasCropped = false;
        if (widthPixels <= 0 || heightPixels <= 0) return File.ReadAllBytes(imagePath);

        try 
        {
            using var original = new Bitmap(imagePath);
            
            // If the image is exactly the requested size or slightly smaller, don't crop
            if (original.Width <= widthPixels + 2 && original.Height <= heightPixels + 2)
            {
                return File.ReadAllBytes(imagePath);
            }

            // We enforce a strict crop to the requested dimensions from top-left.
            // Some drivers scan the whole bed even when given specific Extents.
            int cropW = Math.Min(widthPixels, original.Width);
            int cropH = Math.Min(heightPixels, original.Height);
            
            using var cropped = new Bitmap(cropW, cropH);
            using var g = Graphics.FromImage(cropped);
            g.DrawImage(original, new Rectangle(0, 0, cropW, cropH), new Rectangle(0, 0, cropW, cropH), GraphicsUnit.Pixel);
            
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
