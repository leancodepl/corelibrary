using System;
using System.Reflection;
using System.Text;

namespace LeanCode.CQRS.Cache
{
    static class QueryCacheKeyProvider
    {
        public static string GetKey(IQuery query)
        {
            var type = query.GetType();
            StringBuilder key = new StringBuilder();
            key.Append(type.Name);
            SerializeProperties(key, type, query);
            return key.ToString();
        }

        private static void SerializeProperties(
            StringBuilder builder, Type type, object obj)
        {
            foreach (var prop in type.GetTypeInfo().DeclaredProperties)
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
