using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class DateTimeOnlyConverterTests
{
    private readonly DateOnlyConverter dateConverter = new();
    private readonly TimeOnlyConverter timeConverter = new();

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
}
