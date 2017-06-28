using System;
using System.Reflection;

namespace LeanCode.CQRS
{
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
