namespace PensionMS.Core.Time;

public static class YemenTime
{
    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Asia/Aden");
    public static DateTime Now => LocalTime(TimeProvider.System.GetUtcNow());
    public static DateOnly Today => DateOnly.FromDateTime(Now);
    public static DateTime TodayDateTime => Today.ToDateTime(TimeOnly.MinValue);
    public static DateTime TodayStartUtc => StartUtc(Today);

    public static DateTime LocalTime(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, Zone).DateTime;
    public static DateOnly LocalDate(DateTimeOffset instant) => DateOnly.FromDateTime(LocalTime(instant));
    public static DateTime StartUtc(DateOnly date) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), Zone);

    public static (DateTime From, DateTime To) PeriodUtc(string period, DateOnly? onDate = null)
    {
        var today = onDate ?? Today;
        var start = period.ToLowerInvariant() switch
        {
            "yesterday" => today.AddDays(-1),
            "week" => today.AddDays(-(int)today.DayOfWeek),
            "month" => new DateOnly(today.Year, today.Month, 1),
            "year" => new DateOnly(today.Year, 1, 1),
            _ => today
        };
        return (StartUtc(start), StartUtc(period == "yesterday" ? today : today.AddDays(1)));
    }

    public static int Age(DateOnly birthDate, DateOnly? onDate = null)
    {
        var today = onDate ?? Today;
        var years = today.Year - birthDate.Year;
        return birthDate.AddYears(years) > today ? years - 1 : years;
    }
}
