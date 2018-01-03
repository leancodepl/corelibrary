using System;
using System.Reflection;
using System.Text;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Cache
{
    static class QueryCacheKeyProvider
    {
        public static string GetKey(QueryExecutionPayload payload)
        {
            var contextType = payload.Context.GetType();
            var queryType = payload.Object.GetType();
            StringBuilder key = new StringBuilder();
            key.Append(queryType.Name);
            SerializeProperties(key, contextType, payload.Context);
            key.Append('-');
            SerializeProperties(key, queryType, payload.Object);
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
