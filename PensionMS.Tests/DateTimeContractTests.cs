using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PensionMS.API.Converters;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Tests;

public class DateTimeContractTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        Converters = { new YemenDateTimeConverter() }
    };

    [Theory]
    [InlineData("2026-09-21T20:59:59Z", "2026-09-21")]
    [InlineData("2026-09-21T21:00:00Z", "2026-09-22")]
    [InlineData("2026-12-31T21:00:00Z", "2027-01-01")]
    public void LocalDate_UsesYemenBoundary(string instant, string expected) =>
        Assert.Equal(DateOnly.ParseExact(expected, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            YemenTime.LocalDate(DateTimeOffset.Parse(instant, CultureInfo.InvariantCulture)));

    [Fact]
    public void TodayRange_IsHalfOpenAndStartsAt21Utc()
    {
        var (start, end) = YemenTime.PeriodUtc("today", new DateOnly(2026, 9, 22));
        Assert.Equal(new DateTime(2026, 9, 21, 21, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(start.AddDays(1), end);
        Assert.Equal(DateTimeKind.Utc, end.Kind);
        Assert.Equal(new DateOnly(2026, 9, 22), YemenTime.LocalDate(end.AddTicks(-1)));
        Assert.Equal(new DateOnly(2026, 9, 23), YemenTime.LocalDate(end));
    }

    [Theory]
    [InlineData("week", "2026-09-19T21:00:00Z")]
    [InlineData("month", "2026-08-31T21:00:00Z")]
    [InlineData("year", "2025-12-31T21:00:00Z")]
    public void Periods_StartAtYemenMidnight(string period, string expected) =>
        Assert.Equal(DateTimeOffset.Parse(expected, CultureInfo.InvariantCulture).UtcDateTime,
            YemenTime.PeriodUtc(period, new DateOnly(2026, 9, 22)).From);

    [Theory]
    [InlineData("2008-09-22", "2026-09-21", 17)]
    [InlineData("2008-09-22", "2026-09-22", 18)]
    [InlineData("2000-02-29", "2026-02-28", 26)]
    public void Age_UsesCalendarAnniversary(string birth, string today, int expected) =>
        Assert.Equal(expected, YemenTime.Age(
            DateOnly.ParseExact(birth, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateOnly.ParseExact(today, "yyyy-MM-dd", CultureInfo.InvariantCulture)));

    [Theory]
    [InlineData("\"2026-09-22T00:30:00+03:00\"")]
    [InlineData("\"2026-09-21T21:30:00Z\"")]
    [InlineData("\"2026-09-22T00:30:00\"")]
    public void Instants_RoundTripWithoutAddingThreeHours(string payload)
    {
        var parsed = JsonSerializer.Deserialize<DateTime>(payload, Json);
        Assert.Equal(new DateTime(2026, 9, 21, 21, 30, 0, DateTimeKind.Utc), parsed);
        var serialized = JsonSerializer.Serialize(parsed, Json);
        Assert.EndsWith("Z\"", serialized);
        Assert.Equal(parsed, JsonSerializer.Deserialize<DateTime>(serialized, Json));
    }

    [Fact]
    public void SqlUnspecifiedInstant_IsWrittenAsUtc()
    {
        var value = new DateTime(2026, 9, 21, 21, 30, 0, DateTimeKind.Unspecified);
        Assert.Equal("\"2026-09-21T21:30:00Z\"", JsonSerializer.Serialize(value, Json));
    }

    [Fact]
    public void DateOnly_RoundTripsWithoutAnOffset()
    {
        var date = new DateOnly(1966, 12, 31);
        Assert.Equal("\"1966-12-31\"", JsonSerializer.Serialize(date, Json));
        Assert.Equal(date, JsonSerializer.Deserialize<DateOnly>("\"1966-12-31\"", Json));
    }

    [Fact]
    public void Document_RemainsValidOnExpiryDay()
    {
        var today = YemenTime.Today;
        var document = new DocumentDto { ExpiryDate = today };
        Assert.False(document.IsExpired);
        document.ExpiryDate = today.AddDays(-1);
        Assert.True(document.IsExpired);
    }

    [Fact]
    public void DateOnlyProperties_MapToSqlDateAndQueriesTranslate()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DateContractTests;Trusted_Connection=True")
            .Options);
        var properties = db.Model.GetEntityTypes().SelectMany(e => e.GetProperties())
            .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) == typeof(DateOnly)).ToList();
        Assert.NotEmpty(properties);
        Assert.All(properties, p => Assert.Equal("date", p.GetColumnType()));
        var today = new DateOnly(2026, 9, 22);
        var sql = db.Set<EntitledPerson>().Where(e => e.BirthDate.AddYears(18) > today).ToQueryString();
        Assert.Contains("DATEADD", sql);
        Assert.Contains("ExpiryDate", db.Set<Document>().Where(d => d.ExpiryDate < today).ToQueryString());
    }
}
