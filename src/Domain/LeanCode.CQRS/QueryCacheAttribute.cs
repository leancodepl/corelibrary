using System;
using System.Reflection;

namespace LeanCode.CQRS
{
    /// <summary>
    /// Specifes that query result for <see cref="IQuery{TContext, TResult}" /> should be cached and maximum cache duration
    /// </summary>
    /// <remarks>
    /// Query results are cached with a key. A key is derived from all query and object context properties.
    /// <remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class QueryCacheAttribute : Attribute
    {
        public TimeSpan Duration { get; }

        public QueryCacheAttribute(double durationInSeconds)
        {
            Duration = TimeSpan.FromSeconds(durationInSeconds);
        }

        public static TimeSpan? GetDuration(Type type)
        {
            return type.GetTypeInfo()
                .GetCustomAttribute<QueryCacheAttribute>()
                ?.Duration;
        }

        public static TimeSpan? GetDuration(object obj)
        {
            return GetDuration(obj.GetType());
        }
    }
}
