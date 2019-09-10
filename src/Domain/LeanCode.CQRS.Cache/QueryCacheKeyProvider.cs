using System;
using System.Text;

namespace LeanCode.CQRS.Cache
{
    internal static class QueryCacheKeyProvider
    {
        public static string GetKey<TAppContext>(TAppContext context, IQuery query)
            where TAppContext : notnull
        {
            var queryType = query.GetType();

            return new StringBuilder()
                .Append(queryType.Name)
                .SerializeObject(typeof(TAppContext), context)
                .Append('-')
                .SerializeObject(queryType, query)
                .ToString();
        }

        private static StringBuilder SerializeObject(
            this StringBuilder builder, Type type, object obj)
        {
            if (obj is ICacheKeyProvider provider)
            {
                provider.ProvideKey(builder);
            }
            else
            {
                foreach (var prop in type.GetProperties())
                {
                    builder.Append('-');
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        builder.Append(value.ToString());
                    }
                }
            }

            return builder;
        }
    }
}
