using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data;

namespace PensionMS.API.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdItemKey = "CorrelationId";

    // تعيين مسارات API إلى معرفات الجداول من SystemTables
    private static readonly Dictionary<string, int> RouteToTableId = new(StringComparer.OrdinalIgnoreCase)
    {
        { "retirees", 1 },
        { "retirement-files", 2 },
        { "entitled-persons", 3 },
        { "agents", 4 },
        { "guardians", 5 },
        { "documents", 6 },
        { "document-pledges", 7 },
        { "missing-documents", 8 },
        { "document-exemptions", 9 },
        { "file-notes", 10 },
        { "suspensions", 11 },
        { "salary-recipients", 12 },
        { "salary-batches", 14 },
        { "settlements", 18 },
        { "payment-books", 19 },
        { "users", 21 }
    };

    // تعيين HTTP methods إلى أسماء الإجراءات
    private static readonly Dictionary<string, string> MethodToAction = new(StringComparer.OrdinalIgnoreCase)
    {
        { "POST", "Create" },
        { "PUT", "Update" },
        { "PATCH", "Update" },
        { "DELETE", "Delete" }
    };

    public AuditLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // تسجيل فقط للعمليات الناجحة (2xx) على الطرق الحساسة
        if (!MethodToAction.ContainsKey(context.Request.Method))
            return;

        if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300)
            return;

        try
        {
            var correlationId = GetOrCreateCorrelationId(context);
            // تم نقل منطق الـ AuditLog إلى AuditSaveChangesInterceptor
        }
        catch (Exception)
        {
            // تجاهل
        }
    }

    private static int? GetUserIdFromClaims(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    private static Guid GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdItemKey, out var existing) && existing is Guid guid)
        {
            return guid;
        }

        var newId = Guid.NewGuid();
        context.Items[CorrelationIdItemKey] = newId;
        return newId;
    }
}
