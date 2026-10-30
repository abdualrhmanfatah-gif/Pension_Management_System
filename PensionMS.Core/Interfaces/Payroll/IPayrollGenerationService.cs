using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة توليد عناصر الصرف وفق قواعد الأهلية (إيقاف، مستلم، استحقاق، دفتر صرف).
/// </summary>
public interface IPayrollGenerationService
{
    /// <summary>
    /// يولد عناصر الصرف دون إضافتها أو حفظها أو إرسال إشعار؛ المستدعي يملك الاستبدال والمعاملة.
    /// </summary>
    /// <param name="batch">كشف الصرف (يجب أن يكون بحالة مسودة).</param>
    /// <param name="runAt">وقت التشغيل (للتأريخ الفعال للاستحقاقات).</param>
    /// <param name="userId">معرف المستخدم المنفذ.</param>
    /// <returns>العناصر المُولدة وقائمة المستبعدين مع الأسباب.</returns>
    Task<PayrollGenerationResult> GenerateForBatchAsync(SalaryPaymentBatch batch, DateTime runAt, int userId);
}
