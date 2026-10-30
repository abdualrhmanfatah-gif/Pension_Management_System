using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace PensionMS.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "حدث خطأ داخلي في الخادم";
        List<string>? errors = null;

        switch (exception)
        {
<<<<<<< HEAD
            case BranchScopeAccessException ex:
                statusCode = HttpStatusCode.Forbidden;
                message = ex.Message;
                break;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            // ── Custom business exceptions ──────────────────────
            case BusinessRuleException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            case EntityNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            case ConcurrencyConflictException ex:
                statusCode = HttpStatusCode.Conflict;
                message = ex.Message;
                break;

            // ── Standard .NET exceptions ────────────────────────
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "المورد المطلوب غير موجود";
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "غير مصرح بالوصول";
                break;

            case FluentValidation.ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "فشل التحقق من البيانات";
                errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
                break;

            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = "البيانات المُدخلة غير صالحة";
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException ex:
                statusCode = HttpStatusCode.Conflict;
                var sqlEx = ex.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlEx?.Number == 2601)
                {
                    var duplicateValue = ExtractDuplicateValue(sqlEx.Message);
                    message = !string.IsNullOrEmpty(duplicateValue)
                        ? $"رقم الهوية الوطنية '{duplicateValue}' موجود مسبقاً في النظام"
                        : "رقم الهوية الوطنية موجود مسبقاً في النظام";
                }
                else if (sqlEx?.Number == 2627)
                {
                    message = "السجل المضاف موجود مسبقاً — لا يمكن التكرار";
                }
                else
                {
                    message = ex.InnerException?.Message ?? "تعارض في البيانات — يرجى المحاولة مجدداً";
                }
                break;

            case InvalidOperationException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;
        }

        _logger.LogError(exception, "خطأ: {Method} {Path} — {StatusCode}",
            context.Request.Method, context.Request.Path, (int)statusCode);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = errors != null
            ? ApiResponse<object>.Fail(message, errors)
            : ApiResponse<object>.Fail(message);

        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() == true)
        {
            response.Message += $" | {exception.GetType().Name}: {exception.Message}";
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await context.Response.WriteAsync(json);
    }

    private static string ExtractDuplicateValue(string sqlMessage)
    {
        var match = System.Text.RegularExpressions.Regex.Match(sqlMessage, @"The duplicate key value is \((.+?)\)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
