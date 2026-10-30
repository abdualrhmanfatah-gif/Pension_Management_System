namespace PensionMS.Core.Helpers;

/// <summary>
/// أدوات إخفاء البيانات الحساسة ديناميكياً
/// </summary>
public static class DataMasking
{
    /// <summary>
    /// إخفاء الرقم الوطني — يعرض أول 3 أرقام وآخر 2 فقط
    /// مثال: 12345678901 → 123****01
    /// </summary>
    public static string MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 6)
            return "****";

        return $"{nationalId[..3]}****{nationalId[^2..]}";
    }

    /// <summary>
    /// إخفاء رقم الحساب البنكي — يعرض آخر 4 أرقام فقط
    /// مثال: 123456789012 → ********9012
    /// </summary>
    public static string MaskBankAccount(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 5)
            return "****";

        return $"{"".PadLeft(accountNumber.Length - 4, '*')}{accountNumber[^4..]}";
    }

    /// <summary>
    /// إخفاء رقم الهاتف — يعرض آخر 4 أرقام فقط
    /// مثال: 07701234567 → *******4567
    /// </summary>
    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 5)
            return "****";

        return $"{"".PadLeft(phone.Length - 4, '*')}{phone[^4..]}";
    }
}
