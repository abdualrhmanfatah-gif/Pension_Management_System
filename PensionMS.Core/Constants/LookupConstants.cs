namespace PensionMS.Core.Constants;

public static class LookupConstants
{
    public const int SystemUserId = 1;

    // Statuses
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Pending = "Pending";
    public const string Fulfilled = "Fulfilled";
    public const string Exempted = "Exempted";
    public const string Completed = "Completed";
    
    // Arabic Statuses (Commonly used in DB)
    public const string ActiveAr = "فعال";
    public const string InactiveAr = "غير فعال";
    
    // Recipient Types
    public const string RecipientTypeRetiree = "متقاعد";
    public const string RecipientTypeAgent = "وكيل";
    public const string RecipientTypeEntitled = "مستحق";
    public const string RecipientTypeGuardian = "وصي";
}

public static class TransactionStatusConstants
{
    public const int PendingSend = 1;
    public const int Sent = 2;
    public const int Failed = 3;
    public const int Cancelled = 4;
}
