using System;
using System.Reflection;
using System.Text;

namespace LeanCode.CQRS.Cache
{
    internal static class QueryCacheKeyProvider
    {
        public static string GetKey<TAppContext>(TAppContext context, IQuery query)
        {
            var contextType = typeof(TAppContext);
            var queryType = query.GetType();

            var key = new StringBuilder();
            key.Append(queryType.Name);
            key.SerializeObject(contextType, context);
            key.Append('-');
            key.SerializeObject(queryType, query);
            return key.ToString();
        }

        private static void SerializeObject(
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
        }
    }
}
