using System.Text.Json.Serialization;

namespace LeanCode.DomainModels.Model;

/// <remarks>
/// This type is intended to be used with PostgreSQL which supports AT TIME ZONE operator
/// with IANA time zone IDs but cannot store both timestamp and offset in a single column.
/// </remarks>
public sealed record class TimestampTz : ValueObject
{
    [JsonIgnore]
    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    /// <summary>
    /// Returns contained timestamp in local time as DateTime.
    /// Can be evaluated database-side with PostgreSQL but the offset is lost during conversion.
    /// </summary>
    [JsonIgnore]
    public DateTime LocalTimestampWithoutOffset =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(UtcTimestamp, TimeZoneId).DateTime;

    /// <summary>
    /// Returns contained timestamp in local time as DateTimeOffset.
    /// Cannot be evaluated database-side with PostgreSQL.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset LocalTimestampWithOffset =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(UtcTimestamp, TimeZoneId);

    /// <summary>
    /// Always stored with offset equal to zero.
    /// </summary>
    [JsonInclude]
    public DateTimeOffset UtcTimestamp { get; private init; }

    /// <summary>
    /// Always expressed using IANA identifier.
    /// </summary>
    [JsonInclude]
    public string TimeZoneId { get; private init; }

    public static bool IsValidTimeZoneId(string timeZoneId)
    {
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneId, out var ianaId))
        {
            timeZoneId = ianaId;
        }

        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZoneInfo) && timeZoneInfo.HasIanaId;
    }

    [JsonConstructor]
    private TimestampTz()
    {
        TimeZoneId = default!;
    }

    public TimestampTz(DateTimeOffset timestamp, TimeZoneInfo timeZoneInfo)
        : this(timestamp.UtcDateTime, timeZoneInfo.Id) { }

    public TimestampTz(DateTimeOffset timestamp, string timeZoneId)
        : this(timestamp.UtcDateTime, timeZoneId) { }

    public TimestampTz(DateTime utcTimestamp, TimeZoneInfo timeZoneInfo)
        : this(utcTimestamp, timeZoneInfo.Id) { }

    public TimestampTz(DateTime utcTimestamp, string timeZoneId)
    {
        if (utcTimestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp's Kind must be Utc.", nameof(utcTimestamp));
        }

        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneId, out var ianaId))
        {
            timeZoneId = ianaId;
        }

        if (!TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).HasIanaId)
        {
            throw new ArgumentException("TimeZoneId must have associated IANA identifier.", nameof(timeZoneId));
        }

        UtcTimestamp = new(utcTimestamp, TimeSpan.Zero);
        TimeZoneId = timeZoneId;
    }

    public void Deconstruct(out DateTimeOffset utcTimestamp, out string timeZoneId)
    {
        utcTimestamp = UtcTimestamp;
        timeZoneId = TimeZoneId;
    }
}
