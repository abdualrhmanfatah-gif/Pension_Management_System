using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.API.Extensions;

/// <summary>
/// طرق مساعدة للتحكم بالمستخدم في Controller
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// استخراج معرف المستخدم من JWT (ClaimTypes.NameIdentifier)
    /// </summary>
    public static int GetUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId) || userId <= 0)
            throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم");
        return userId;
    }

    /// <summary>
    /// استخراج معرف الفرع من JWT
    /// </summary>
    public static int GetBranchId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst("BranchId");
        return (claim != null && int.TryParse(claim.Value, out int branchId)) ? branchId : 0;
    }

    /// <summary>
    /// هل المستخدم من الإدارة العامة (المركز الرئيسي)؟
    /// </summary>
    public static bool IsHQ(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst("IsHQ");
        return claim != null && bool.TryParse(claim.Value, out bool isHq) && isHq;
    }

    /// <summary>
    /// تحويل ApiResponse إلى IActionResult مناسب
    /// </summary>
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
    {
        if (response.IsSuccess)
            return new OkObjectResult(response);

        // يمكن تخصيص نوع الخطأ بناءً على الرسالة أو وجود أخطاء تفصيلية
        if (response.Message != null && (response.Message.Contains("غير موجود") || response.Message.Contains("Not Found")))
            return new NotFoundObjectResult(response);

        return new BadRequestObjectResult(response);
    }
}
