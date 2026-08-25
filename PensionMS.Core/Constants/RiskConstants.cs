namespace PensionMS.Core.Constants;

/// <summary>
/// ثوابت المخاطر والامتثال.
/// </summary>
public static class RiskConstants
{
    public const string Open = "Open";
    public const string Investigating = "Investigating";
    public const string Resolved = "Resolved";

    public static class Types
    {
        public const string AfterHoursAction = "AfterHoursAction";
        public const string HighValueEntitlement = "HighValueEntitlement";
    }
    
    public static class Severity
    {
        public const string Critical = "Critical";
        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";
    }
}
