using System.Linq.Expressions;
using System.Reflection;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF;

public sealed class TimestampTzExpressionRewriter : ExpressionVisitor
{
    private static readonly Type TimestampTzType = typeof(TimestampTz);
    private static readonly Type TimeZoneInfoType = typeof(TimeZoneInfo);
    private static readonly Type DateTimeType = typeof(DateTime);
    private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);

    public static readonly PropertyInfo LocalTimestampWithoutOffsetProperty =
        TimestampTzType.GetProperty(nameof(TimestampTz.LocalTimestampWithoutOffset))
        ?? throw new MissingMemberException(TimestampTzType.FullName, nameof(TimestampTz.LocalTimestampWithoutOffset));

    public static readonly PropertyInfo UtcTimestampProperty =
        TimestampTzType.GetProperty(nameof(TimestampTz.UtcTimestamp))
        ?? throw new MissingMemberException(TimestampTzType.FullName, nameof(TimestampTz.UtcTimestamp));

    public static readonly PropertyInfo TimeZoneIdProperty =
        TimestampTzType.GetProperty(nameof(TimestampTz.TimeZoneId))
        ?? throw new MissingMemberException(TimestampTzType.FullName, nameof(TimestampTz.TimeZoneId));

    public static readonly PropertyInfo UtcDateTimeProperty =
        DateTimeOffsetType.GetProperty(nameof(DateTimeOffset.UtcDateTime))
        ?? throw new MissingMemberException(DateTimeOffsetType.FullName, nameof(DateTimeOffset.UtcDateTime));

    public static readonly MethodInfo ToUniversalTimeMethod =
        DateTimeType.GetMethod(nameof(DateTime.ToUniversalTime), Type.EmptyTypes)
        ?? throw new MissingMemberException(DateTimeType.FullName, nameof(DateTime.ToUniversalTime));

    public static readonly MethodInfo ConvertDateTimeBySystemTimeZoneIdMethod =
        TimeZoneInfoType.GetMethod(
            nameof(TimeZoneInfo.ConvertTimeBySystemTimeZoneId),
            new[] { typeof(DateTime), typeof(string) }
        )
        ?? throw new MissingMemberException(
            TimeZoneInfoType.FullName,
            nameof(TimeZoneInfo.ConvertTimeBySystemTimeZoneId)
        );

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member == LocalTimestampWithoutOffsetProperty)
        {
            return Expression.Call(
                null,
                ConvertDateTimeBySystemTimeZoneIdMethod,
                // Npgsql 11 translates UtcDateTime as a no-op which retains DateTimeOffset as the SQL
                // expression's CLR type. ToUniversalTime retags it as DateTime, which the outer translator requires.
                Expression.Call(
                    Expression.Property(
                        Expression.Property(node.Expression, UtcTimestampProperty),
                        UtcDateTimeProperty
                    ),
                    ToUniversalTimeMethod
                ),
                Expression.Property(node.Expression, TimeZoneIdProperty)
            );
        }
        else
        {
            return node;
        }
    }
}

public static class TimestampTzExpressionInterceptorDbContextOptionsBuilderExtensions
{
    public static readonly QueryExpressionVisitorInterceptor Interceptor = new(new TimestampTzExpressionRewriter());

    public static DbContextOptionsBuilder AddTimestampTzExpressionInterceptor(
        this DbContextOptionsBuilder optionsBuilder
    )
    {
        optionsBuilder.AddInterceptors(Interceptor);
        return optionsBuilder;
    }
}
