namespace PensionMS.Core.Constants;

public static class MissingDocumentStatusConstants
{
    public const string Pending = "Pending";       // ناقص
    public const string Pledged = "Pledged";       // متعهد به
    public const string Fulfilled = "Fulfilled";  // مسلم
    public const string Exempted = "Exempted";     // معفى
    public const string PendingPledge = "بانتظار التعهد";  // بانتظار تقديم التعهد
    public const string ExemptedAr = "معفى";               // معفى (عربي)

    // النواقص الفعلية التي ننتظر الحصول عليها في الملف
    public static readonly IReadOnlyList<string> OutstandingStatuses = new[] { Pending, Pledged };
}
