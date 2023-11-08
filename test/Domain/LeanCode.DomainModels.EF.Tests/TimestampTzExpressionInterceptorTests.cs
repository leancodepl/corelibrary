using System.Linq.Expressions;
using FluentAssertions;
using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class TimestampTzExpressionInterceptorTests
{
    [Fact]
    public void LocalTimestampWithoutOffsetProperty_accesses_expected_property()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");

        TimestampTzExpressionRewriter
            .LocalTimestampWithoutOffsetProperty
            .GetGetMethod()
            .Invoke(timestampTz, null)
            .Should()
            .Be(timestampTz.LocalTimestampWithoutOffset);
    }

    [Fact]
    public void UtcTimestampProperty_accesses_expected_property()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");

        TimestampTzExpressionRewriter
            .UtcTimestampProperty
            .GetGetMethod()
            .Invoke(timestampTz, null)
            .Should()
            .Be(timestampTz.UtcTimestamp);
    }

    [Fact]
    public void TimeZoneIdProperty_accesses_expected_property()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");

        TimestampTzExpressionRewriter
            .TimeZoneIdProperty
            .GetGetMethod()
            .Invoke(timestampTz, null)
            .Should()
            .Be(timestampTz.TimeZoneId);
    }

    [Fact]
    public void UtcDateTimeProperty_accesses_expected_property()
    {
        var dto = DateTimeOffset.Now;

        TimestampTzExpressionRewriter.UtcDateTimeProperty.GetGetMethod().Invoke(dto, null).Should().Be(dto.UtcDateTime);
    }

    [Fact]
    public void ConvertDateTimeBySystemTimeZoneIdMethod_calls_expected_method()
    {
        var utcNow = DateTime.UtcNow;

        TimestampTzExpressionRewriter
            .ConvertDateTimeBySystemTimeZoneIdMethod
            .Invoke(null, [ utcNow, "Europe/Warsaw" ])
            .Should()
            .Be(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcNow, "Europe/Warsaw"));
    }

    [Fact]
    public void Interceptor_rewrites_expression_tree_as_expected()
    {
        Expression input = (TimestampTz tstz) => tstz.LocalTimestampWithoutOffset;

        Expression expectedOutput = (TimestampTz tstz) =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(tstz.UtcTimestamp.UtcDateTime, tstz.TimeZoneId);

        TimestampTzExpressionInterceptorDbContextOptionsBuilderExtensions
            .Interceptor
            .QueryCompilationStarting(input, default)
            .Should()
            .BeEquivalentTo(expectedOutput);
    }
}
