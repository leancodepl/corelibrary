using System;
using System.Reflection;
using System.Text;

namespace LeanCode.CQRS.Cache
{
    static class QueryCacheKeyProvider
    {
        public static string GetKey<TAppContext>(TAppContext context, IQuery query)
        {
            var contextType = typeof(TAppContext);
            var queryType = query.GetType();
            StringBuilder key = new StringBuilder();
            key.Append(queryType.Name);
            SerializeProperties(key, contextType, context);
            key.Append('-');
            SerializeProperties(key, queryType, query);
            return key.ToString();
        }

        private static void SerializeProperties(
            StringBuilder builder, Type type, object obj)
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
