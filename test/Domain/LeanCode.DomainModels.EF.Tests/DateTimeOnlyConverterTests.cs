using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

[SuppressMessage("?", "EF1001", Justification = "Tests.")]
public class DateTimeOnlyConverterTests
{
    private readonly DateOnlyConverter dateConverter = new();
    private readonly DateOnlyComparer dateComparer = new();
    private readonly TimeOnlyConverter timeConverter = new();
    private readonly TimeOnlyComparer timeComparer = new();

    [Fact]
    public void DateOnly_conversion_to_DateTime_and_back_works()
    {
        AssertConvertsDate(new(2022, 3, 10), new(2022, 3, 10, 0, 0, 0));
        AssertConvertsDate(new(2032, 3, 10), new(2032, 3, 10, 0, 0, 0));
        AssertConvertsDate(new(2002, 3, 10), new(2002, 3, 10, 0, 0, 0));

        // Drops time part
        AssertConvertsDateBack(new(2002, 3, 10, 10, 0, 0), new(2002, 3, 10));
        AssertConvertsDateBack(new(2002, 3, 10, 1, 0, 0), new(2002, 3, 10));
    }

    [Fact]
    public void TimeOnly_conversion_to_TimeSpan_and_back_works()
    {
        AssertConvertsTime(new(1, 2, 3, 4), new(0, 1, 2, 3, 4));
        AssertConvertsTime(new(1, 2, 3, 0), new(0, 1, 2, 3, 0));
        AssertConvertsTime(new(1, 2, 0, 0), new(0, 1, 2, 0, 0));
        AssertConvertsTime(new(1, 2, 3, 999), new(0, 1, 2, 3, 999));
        AssertConvertsTime(new(23, 59, 59, 999), new(0, 23, 59, 59, 999));

        // Values over 24h won't ever work
        Assert.Throws<ArgumentOutOfRangeException>(() => timeConverter.ConvertFromProvider(new TimeSpan(1, 0, 0, 0)));
    }

    [Fact]
    public void Date_comparison_behaves_correctly()
    {
        AssertDateEqual(new(2022, 3, 10));
        AssertDateEqual(new(2022, 3, 11));
        AssertDateEqual(new(2137, 2, 13));

        AssertDateNotEqual(new(2022, 3, 10), new(2022, 3, 11));
        AssertDateNotEqual(new(2022, 3, 11), new(2022, 3, 10));
    }

    [Fact]
    public void Time_comparison_behaves_correctly()
    {
        AssertTimeEqual(new(22, 36, 02));
        AssertTimeEqual(new(11, 11, 11, 999));

        AssertTimeNotEqual(new(11, 11, 11), new(12, 12, 12));
        AssertTimeNotEqual(new(12, 12, 12), new(11, 11, 11));
    }

    [Fact]
    public void Date_convention_is_registered_properly_if_type_is_specified()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.RegisterDateTimeOnlyTypes();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(DateOnly));
        Assert.NotNull(mapping);
        Assert.IsType<DateOnlyConverter>(mapping.GetValueConverter());
        Assert.Equal(typeof(DateOnlyComparer), mapping["ValueComparerType"]);
        Assert.Equal(typeof(DateOnly), mapping.ClrType);
        Assert.Equal("date", mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void Date_convention_is_registered_properly_if_type_is_not_specified()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.RegisterDateTimeOnlyTypes(dateType: null);
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(DateOnly));
        Assert.NotNull(mapping);
        Assert.IsType<DateOnlyConverter>(mapping.GetValueConverter());
        Assert.Equal(typeof(DateOnlyComparer), mapping["ValueComparerType"]);
        Assert.Equal(typeof(DateOnly), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void Time_convention_is_registered_properly_if_type_is_specified()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.RegisterDateTimeOnlyTypes();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(TimeOnly));
        Assert.NotNull(mapping);
        Assert.IsType<TimeOnlyConverter>(mapping.GetValueConverter());
        Assert.Equal(typeof(TimeOnlyComparer), mapping["ValueComparerType"]);
        Assert.Equal(typeof(TimeOnly), mapping.ClrType);
        Assert.Equal("time", mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void Time_convention_is_registered_properly_if_type_is_not_specified()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.RegisterDateTimeOnlyTypes(timeType: null);
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(TimeOnly));
        Assert.NotNull(mapping);
        Assert.IsType<TimeOnlyConverter>(mapping.GetValueConverter());
        Assert.Equal(typeof(TimeOnlyComparer), mapping["ValueComparerType"]);
        Assert.Equal(typeof(TimeOnly), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    private void AssertConvertsDate(DateOnly date, DateTime dateTime)
    {
        var toResult = dateConverter.ConvertToProvider(date);
        var fromResult = dateConverter.ConvertFromProvider(dateTime);
        Assert.Equal(dateTime, toResult);
        Assert.Equal(date, fromResult);
    }

    private void AssertConvertsDateBack(DateTime dateTime, DateOnly date)
    {
        var fromResult = dateConverter.ConvertFromProvider(dateTime);
        Assert.Equal(date, fromResult);
    }

    private void AssertConvertsTime(TimeOnly time, TimeSpan timeSpan)
    {
        var toResult = timeConverter.ConvertToProvider(time);
        var fromResult = timeConverter.ConvertFromProvider(timeSpan);
        Assert.Equal(timeSpan, toResult);
        Assert.Equal(time, fromResult);
    }

    private void AssertDateEqual(DateOnly date)
    {
        Assert.True(dateComparer.Equals(date, date));
        Assert.Equal(dateComparer.GetHashCode(date), dateComparer.GetHashCode(date));
        Assert.Equal(dateComparer.Snapshot(date), date);
    }

    private void AssertDateNotEqual(DateOnly date1, DateOnly date2)
    {
        Assert.False(dateComparer.Equals(date1, date2));
        Assert.NotEqual(dateComparer.GetHashCode(date1), dateComparer.GetHashCode(date2));

        Assert.Equal(dateComparer.Snapshot(date1), date1);
        Assert.Equal(dateComparer.Snapshot(date2), date2);
    }

    private void AssertTimeEqual(TimeOnly time)
    {
        Assert.True(timeComparer.Equals(time, time));
        Assert.Equal(timeComparer.GetHashCode(time), timeComparer.GetHashCode(time));
        Assert.Equal(timeComparer.Snapshot(time), time);
    }

    private void AssertTimeNotEqual(TimeOnly time1, TimeOnly time2)
    {
        Assert.False(timeComparer.Equals(time1, time2));
        Assert.NotEqual(timeComparer.GetHashCode(time1), timeComparer.GetHashCode(time2));

        Assert.Equal(timeComparer.Snapshot(time1), time1);
        Assert.Equal(timeComparer.Snapshot(time2), time2);
    }

    private class ModelConfigurationBuilderWrapper : ModelConfigurationBuilder
    {
        public ModelConfigurationBuilderWrapper()
            : base(new(), new ServiceCollection().BuildServiceProvider())
        { }

        public ModelConfiguration Build() => ModelConfiguration;
    }
}
