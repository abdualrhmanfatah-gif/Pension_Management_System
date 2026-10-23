using PensionMS.Core.Entities;
using PensionMS.Core.DTOs.Suspensions;

namespace PensionMS.Core.Constants;

/// <summary>تسمية حالة المعاش للعرض؛ لا تحل محل فحوص تنفيذ الصرف.</summary>
public static class PensionSalaryStatusRules
{
    public static PaymentHoldCheckDto Evaluate(int? fileStatus, FileReviewStatus reviewStatus, bool hasActiveSuspension)
    {
        if (fileStatus == FileStatusConstants.EntitlementEnded)
            return new(true, "انتهى استحقاق الملف", "انتهى الاستحقاق");
        if (fileStatus != FileStatusConstants.Active)
            return new(true, "الملف غير نشط للصرف", "موقوف");
        if (reviewStatus != FileReviewStatus.Approved)
            return new(true, "مراجعة الملف غير معتمدة", "موقوف");
        if (hasActiveSuspension)
            return new(true, "يوجد إيقاف معاش نشط", "موقوف");
        return new(false, null, "قابل للصرف");
    }

    public static string Describe(int? fileStatus, FileReviewStatus reviewStatus, bool hasActiveSuspension) =>
        Evaluate(fileStatus, reviewStatus, hasActiveSuspension).SalaryStatus;
}
