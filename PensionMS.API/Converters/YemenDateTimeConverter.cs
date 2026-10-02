using System.Text.Json;
using System.Text.Json.Serialization;

namespace PensionMS.API.Converters;

public class YemenDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!reader.TryGetDateTime(out var value))
            throw new JsonException("صيغة التاريخ والوقت غير صحيحة.");
        // Unmigrated calendar fields still use DateTime internally.
        if (reader.GetString()!.Length == 10) return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        // Legacy unqualified values represent Yemen wall time on input.
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(value, PensionMS.Core.Time.YemenTime.Zone)
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // SQL datetime2 loses Kind; persisted instants are UTC.
        writer.WriteStringValue(value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
