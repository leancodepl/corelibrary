# TimestampTz

`TimestampTz` is our custom [value object](../value_object/index.md) type created to address the inability to persist a timestamp in particular time zone (or with any non-zero offset) in a single column in Postgres. As opposed to .NET's and Microsoft SQL Server's `DateTimeOffset`, it takes a different approach and stores the UTC timestamp and the IANA ID of some time zone.

Recommended reading for some additional context: [Storing UTC is not a silver bullet](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels) | `TimestampTz` |
| LeanCode.DomainModels.EF | [![NuGet version (LeanCode.DomainModels.EF)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.EF.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels.EF) | `TimestampTzExpressionInterceptorDbContextOptionsBuilderExtensions` |

## Definition

The type exposes the following public API:

```csharp
public sealed record class TimestampTz : ValueObject
{
    public TimeZoneInfo TimeZoneInfo { get; }
    public DateTime LocalTimestampWithoutOffset { get; }
    public DateTimeOffset LocalTimestampWithOffset { get; }
    public DateTimeOffset UtcTimestamp { get; }
    public string TimeZoneId { get; }
    public static bool IsValidTimeZoneId(string timeZoneId);
    public TimestampTz(DateTimeOffset timestamp, TimeZoneInfo timeZoneInfo);
    public TimestampTz(DateTimeOffset timestamp, string timeZoneId);
    public TimestampTz(DateTime utcTimestamp, TimeZoneInfo timeZoneInfo);
    public TimestampTz(DateTime utcTimestamp, string timeZoneId);
    public void Deconstruct(out DateTimeOffset utcTimestamp, out string timeZoneId);
}
```

The only properties that have backing fields, are mapped to database columns, and are written when serializing to JSON, are `UtcTimestamp` and `TimeZoneId`. The remaining properties are computed from these two, with `LocalTimestampWithoutOffset` being supported on the database side in EF queries (translated to `"UtcTimestamp" AT TIME ZONE "TimeZoneId"` if the query expression interceptor is installed, see below).

Constructors will perform conversion of provided `DateTimeOffset`s to UTC and time zone IDs to IANA IDs, and throw when given `DateTime` of `Kind` other than `DateTimeKind.Utc`. Once constructed and initialized, the type is immutable.

## Recommended usage

As a value object, the type will most often be used as a property in some entity or another value object. To have Entity Framework correctly map this type to two separate columns, it's recommended to use the [`ComplexProperty` API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1.complexproperty) to configure the database model.

Additionally, if database-side support for `LocalTimestampWithoutOffset` is desired, use `TimestampTzExpressionInterceptorDbContextOptionsBuilderExtensions.AddTimestampTzExpressionInterceptor(this DbContextOptionsBuilder)` to install a query expression interceptor that will replace reads of that property with equivalent expression that EF will be able to translate to SQL.

When reading, usually converting to `LocalTimestampWithOffset` and returning only that to clients might be sufficient. When doing so, remember that `LocalTimestampWithOffset` cannot be evaluated on Postgres side, so avoid using this property in queries in contexts where its value would participate in some computation, e.g. sorting.

On the other hand, when writing and when replicating saved inputs for readback, it might be desirable to have both properties in the contract. In such case, a DTO like

```csharp
public record TimestampTzDTO(DateTimeOffset Timestamp, string TimeZoneId)
{
    public class ErrorCodes
    {
        public const int InvalidTimeZoneId = 10001;
    }
}
```

should satisfy those needs, with mapping between the two being trivial.

Because EF currently does not support including properties of nested objects in index definitions, indexing timestamps stored in `TimestampTz` requires building indexes manually, using [`MigrationBuilder.CreateIndex`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.migrations.migrationbuilder.createindex) (when there's no need to use [indexes on expressions](https://www.postgresql.org/docs/current/indexes-expressional.html)) or [`MigrationBuilder.Sql`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.migrations.migrationbuilder.sql) APIs, for example:

```csharp
migrationBuilder.Sql(
    """
    create index "IX_Events_StartTime" on "public"."Events"
    (("StartTime_UtcTimestamp" at time zone "StartTime_TimeZoneId"));
    """
);
```

## Limitations

* This type does not account for the variable nature of time zone rules. When attempting to store a point of time when some future event is supposed to take place, keep in mind that it may be defined by the _local_ time in some time zone. If rules governing that time zone change, that local time may no longer map to the same instant. In such cases, consider additionally storing the local timestamp in a separate column. See the blog post linked at the top for details.
* `LocalTimestampWithOffset` cannot be evaluated in Postgres. Therefore, avoid writing queries that do things like ordering by this property, or otherwise rely on it being usable DB-side. However, as with other properties that aren't understood by EF, it can be used in the query's final projection, simply causing client-side evaluation.
* Support for different calendar systems is out of scope.

## FAQ

### Q: When to use `DateTime`, `DateTimeOffset` and `TimestampTz` in domain entities?

A: In Postgres, use `DateTimeOffset` (mapped to `timestamp with time zone` column type) to represent an instant without associated time zone (e.g. various `CreatedAt` timestamps generated on the backend). Use `TimestampTz` to persist an instant in context of some time zone (e.g. in a system aggregating news, it could be a time of an event that took place in some location). Use `DateTime` mapped to `timestamp without time zone` column type _together with_ `TimestampTz` to represent the local time of some event, often one meant to start in the future, or for indexing purposes. Avoid using `DateTime` mapped to `timestamp with time zone` columns: it has no benefits over using `DateTimeOffset`.
