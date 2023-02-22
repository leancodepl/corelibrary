using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(d => d.ToDateTime(TimeOnly.MinValue), d => DateOnly.FromDateTime(d)) { }
}

public class DateOnlyComparer : ValueComparer<DateOnly>
{
    public DateOnlyComparer()
        : base((d1, d2) => d1 == d2 && d1.DayNumber == d2.DayNumber, d => d.GetHashCode()) { }
}

public class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
{
    public TimeOnlyConverter()
        : base(d => d.ToTimeSpan(), d => TimeOnly.FromTimeSpan(d)) { }
}

public class TimeOnlyComparer : ValueComparer<TimeOnly>
{
    public TimeOnlyComparer()
        : base((t1, t2) => t1.Ticks == t2.Ticks, d => d.GetHashCode()) { }
}

public static class ModelConfigurationBuilderDateTimeExtensions
{
    public static ModelConfigurationBuilder RegisterDateTimeOnlyTypes(
        this ModelConfigurationBuilder builder,
        string? dateType = "date",
        string? timeType = "time"
    )
    {
        var dateBuilder = builder.Properties<DateOnly>().HaveConversion<DateOnlyConverter, DateOnlyComparer>();
        if (dateType is not null)
        {
            dateBuilder.HaveColumnType(dateType);
        }

        var timeBuilder = builder.Properties<TimeOnly>().HaveConversion<TimeOnlyConverter, TimeOnlyComparer>();
        if (timeType is not null)
        {
            timeBuilder.HaveColumnType(timeType);
        }

        return builder;
    }
}
