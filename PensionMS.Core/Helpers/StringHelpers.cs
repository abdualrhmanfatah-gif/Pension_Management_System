namespace PensionMS.Core.Helpers;

public static class StringHelpers
{
    /// <summary>
    /// Converts Arabic/Persian numerals to Latin (English) numerals.
    /// </summary>
    public static string? NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.Replace("٠", "0").Replace("١", "1").Replace("٢", "2").Replace("٣", "3").Replace("٤", "4")
                    .Replace("٥", "5").Replace("٦", "6").Replace("٧", "7").Replace("٨", "8").Replace("٩", "9")
                    .Trim();
    }
}
